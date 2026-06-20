/*
 * tersafe_hook.c
 *
 * frida-gum (C/C++) 版本，等价于原 Frida JS 脚本：
 *   - 等待 libtersafe.so 加载（hook android_dlopen_ext / dlopen 的 onLeave）
 *   - hook tss_sdk_decryptpacket / tss_sdk_encryptpacket，打印 in/out 缓冲区
 *   - hook tss_sdk_set_token / tss_sdk_setuserinfo / tss_sdk_gen_session_data
 *   - 预留 AES setkey 偏移 hook
 *
 * 这是一个 frida-gum 内嵌(embedded)注入库，构建成 .so 后注入目标进程即可。
 * 它使用与 JS 版完全相同的 Interceptor 机制，只是改用 gum 的 C API。
 * 基于 frida-gum 17.x 的 API（gum_make_call_listener / GumModule 对象）。
 *
 * 构建（ARM64 Android 为例，需 frida-gum devkit: libfrida-gum.a + frida-gum.h）：
 *   $NDK/.../aarch64-linux-android<API>-clang \
 *       -shared -fPIC -O2 -ffunction-sections -fdata-sections \
 *       -I/path/to/frida-gum-devkit \
 *       tersafe_hook.c \
 *       /path/to/frida-gum-devkit/libfrida-gum.a \
 *       -o libtersafe_hook.so -llog -ldl -lpthread
 *
 * 桌面 Linux 测试：
 *   gcc -shared -fPIC -I<devkit> tersafe_hook.c <devkit>/libfrida-gum.a \
 *       -o libtersafe_hook.so -ldl -lpthread -lm -lresolv -lrt
 */

#include "frida-gum.h"

#include <stdio.h>
#include <stdint.h>
#include <string.h>
#include <stdarg.h>

/* ------------------------------------------------------------------ */
/* 日志：同时输出到 logcat/stdout 和文件 DUMP_PATH                       */
/* ------------------------------------------------------------------ */
#if defined(__ANDROID__)
#include <android/log.h>
#endif

/* 密钥/明文 dump 文件路径 */
#define DUMP_PATH "/data/local/tmp/tersafe_dump.log"

static FILE * g_dump_fp = NULL;

/* 统一日志：写 logcat/stdout，并追加到 DUMP_PATH（自动补换行） */
static void
log_line (const char * fmt, ...)
{
  va_list ap;
  gchar * msg;

  va_start (ap, fmt);
  msg = g_strdup_vprintf (fmt, ap);
  va_end (ap);

#if defined(__ANDROID__)
  __android_log_print (ANDROID_LOG_INFO, "tersafe_hook", "%s", msg);
#else
  printf ("%s\n", msg);
  fflush (stdout);
#endif

  if (g_dump_fp == NULL)
    g_dump_fp = fopen (DUMP_PATH, "a");
  if (g_dump_fp != NULL)
  {
    fputs (msg, g_dump_fp);
    fputc ('\n', g_dump_fp);
    fflush (g_dump_fp);
  }

  g_free (msg);
}

#define LOGI(...) log_line (__VA_ARGS__)

#define TARGET_MODULE "libtersafe.so"

/* ------------------------------------------------------------------ */
/* hexdump_ptr：等价于 JS 的 hexdump_ptr，打印偏移/十六进制/ASCII          */
/* ------------------------------------------------------------------ */
static void
hexdump_ptr (gconstpointer p, gint len, const char * tag)
{
  if (p == NULL || len <= 0 || len > 0x10000)
  {
    LOGI ("%s <skip ptr=%p len=%d>", tag, p, len);
    return;
  }

  const guint8 * data = (const guint8 *) p;
  GString * out = g_string_new (NULL);
  g_string_append_printf (out, "%s (%d bytes):\n", tag, len);

  for (gint off = 0; off < len; off += 16)
  {
    g_string_append_printf (out, "%08x  ", off);

    for (gint i = 0; i < 16; i++)
    {
      if (off + i < len)
        g_string_append_printf (out, "%02x ", data[off + i]);
      else
        g_string_append (out, "   ");
      if (i == 7)
        g_string_append_c (out, ' ');
    }

    g_string_append (out, " |");
    for (gint i = 0; i < 16 && off + i < len; i++)
    {
      guint8 c = data[off + i];
      g_string_append_c (out, (c >= 0x20 && c < 0x7f) ? (char) c : '.');
    }
    g_string_append (out, "|\n");
  }

  LOGI ("%s", out->str);
  g_string_free (out, TRUE);
}

/* 读取目标地址处的 int32，失败返回 0（等价 JS 的 try/catch readInt32） */
static gint
safe_read_int32 (gpointer addr)
{
  gint value = 0;
  gsize n_read = 0;
  guint8 * buf;

  if (addr == NULL)
    return 0;

  buf = gum_memory_read (addr, sizeof (value), &n_read);
  if (buf != NULL && n_read == sizeof (value))
    memcpy (&value, buf, sizeof (value));
  g_free (buf);

  return value;
}

/* ------------------------------------------------------------------ */
/* hook 配置：通过 listener_function_data 传给统一的 listener            */
/* ------------------------------------------------------------------ */
typedef enum
{
  HOOK_PACKET,       /* encryptpacket / decryptpacket            */
  HOOK_INFO,         /* set_token / setuserinfo / gen_session    */
  HOOK_AES_SETKEY,   /* AES setkey 偏移                          */
  HOOK_DLOPEN_WATCH  /* 等待目标模块加载                          */
} HookKind;

typedef struct
{
  HookKind kind;
  const char * label;
} HookConfig;

/* 每次调用在 onEnter/onLeave 之间共享的状态（对应 JS 里的 this.xxx） */
typedef struct
{
  const char * label;
  gpointer in;
  gint inlen;
  gpointer out;
  gpointer outlen_ptr;
} PacketState;

static GumInterceptor * g_interceptor = NULL;
static GumInvocationListener * g_listener = NULL;
static gboolean g_hooks_installed = FALSE;

static void install_hooks (GumModule * module);

/* ------------------------------------------------------------------ */
/* listener 回调（等价 JS 的 onEnter / onLeave）                        */
/* ------------------------------------------------------------------ */
static void
on_enter (GumInvocationContext * ic, gpointer user_data)
{
  HookConfig * cfg = GUM_IC_GET_FUNC_DATA (ic, HookConfig *);
  (void) user_data;

  switch (cfg->kind)
  {
    case HOOK_PACKET:
    {
      PacketState * st = GUM_IC_GET_INVOCATION_DATA (ic, PacketState);
      char tag[64];
      /* 常见布局: x0=ctx, x1=in, x2=inlen, x3=out, x4=outlenPtr */
      st->label = cfg->label;
      st->in = gum_invocation_context_get_nth_argument (ic, 1);
      st->inlen = (gint) GPOINTER_TO_SIZE (gum_invocation_context_get_nth_argument (ic, 2));
      st->out = gum_invocation_context_get_nth_argument (ic, 3);
      st->outlen_ptr = gum_invocation_context_get_nth_argument (ic, 4);

      LOGI ("\n===== %s onEnter =====", st->label);
      g_snprintf (tag, sizeof (tag), "%s arg1(in)", st->label);
      hexdump_ptr (st->in, st->inlen, tag);
      break;
    }

    case HOOK_INFO:
    {
      gpointer a1 = gum_invocation_context_get_nth_argument (ic, 1);
      gpointer a2 = gum_invocation_context_get_nth_argument (ic, 2);
      gpointer a3 = gum_invocation_context_get_nth_argument (ic, 3);
      gpointer args[3] = { a1, a2, a3 };
      LOGI ("\n[%s] x1=%p x2=%p x3=%p", cfg->label, a1, a2, a3);
      for (int i = 0; i < 3; i++)
      {
        char tag[64];
        g_snprintf (tag, sizeof (tag), "%s arg%d", cfg->label, i + 1);
        hexdump_ptr (args[i], 64, tag);
      }
      break;
    }

    case HOOK_AES_SETKEY:
    {
      LOGI ("\n[AES setkey %s]", cfg->label);
      hexdump_ptr (gum_invocation_context_get_nth_argument (ic, 0), 32, "AES key");
      break;
    }

    case HOOK_DLOPEN_WATCH:
    default:
      break;
  }
}

static void
on_leave (GumInvocationContext * ic, gpointer user_data)
{
  HookConfig * cfg = GUM_IC_GET_FUNC_DATA (ic, HookConfig *);
  (void) user_data;

  switch (cfg->kind)
  {
    case HOOK_PACKET:
    {
      PacketState * st = GUM_IC_GET_INVOCATION_DATA (ic, PacketState);
      gpointer ret = gum_invocation_context_get_return_value (ic);
      gint outlen = safe_read_int32 (st->outlen_ptr);
      char tag[64];

      LOGI ("===== %s onLeave ret=%p outlen=%d =====", st->label, ret, outlen);
      g_snprintf (tag, sizeof (tag), "%s arg3(out)", st->label);
      hexdump_ptr (st->out, (outlen != 0) ? outlen : st->inlen, tag);
      break;
    }

    case HOOK_DLOPEN_WATCH:
    {
      /* 每次 dlopen 返回后检查目标模块是否就绪，等价 JS waitForModule */
      if (!g_hooks_installed)
      {
        GumModule * mod = gum_process_find_module_by_name (TARGET_MODULE);
        if (mod != NULL)
        {
          install_hooks (mod);
          g_object_unref (mod);
        }
      }
      break;
    }

    case HOOK_INFO:
    case HOOK_AES_SETKEY:
    default:
      break;
  }
}

/* ------------------------------------------------------------------ */
/* hookExport：找到导出符号并 attach（等价 JS hookExport）              */
/* ------------------------------------------------------------------ */
static void
hook_export (GumModule * module, const char * symbol, HookConfig * cfg)
{
  GumAttachOptions options = { 0, };
  gpointer addr = GSIZE_TO_POINTER (
      gum_module_find_export_by_name (module, symbol));

  if (addr == NULL)
  {
    LOGI ("[!] export not found: %s", symbol);
    return;
  }
  LOGI ("[+] hook %s @ %p", symbol, addr);

  options.listener_function_data = cfg;
  gum_interceptor_attach (g_interceptor, addr, g_listener, &options);
}

/* AES setkey 候选偏移（对应 JS 的 AES_SETKEY_CANDIDATES，例: 0x49FB00） */
static const gsize AES_SETKEY_CANDIDATES[] = {
  /* 0x49FB00, */
};

/* 各 hook 的静态配置（生命周期需长于 attach，所以用 static） */
static HookConfig cfg_decrypt   = { HOOK_PACKET, "DECRYPT" };
static HookConfig cfg_encrypt   = { HOOK_PACKET, "ENCRYPT" };
static HookConfig cfg_set_token = { HOOK_INFO,   "tss_sdk_set_token" };
static HookConfig cfg_setuser   = { HOOK_INFO,   "tss_sdk_setuserinfo" };
static HookConfig cfg_gensess   = { HOOK_INFO,   "tss_sdk_gen_session_data" };
static HookConfig cfg_aes       = { HOOK_AES_SETKEY, "candidate" };

static void
install_hooks (GumModule * module)
{
  GumAddress base;

  if (g_hooks_installed)
    return;
  g_hooks_installed = TRUE;

  base = gum_module_get_range (module)->base_address;
  LOGI ("[*] %s @ 0x%" G_GINT64_MODIFIER "x", TARGET_MODULE, base);

  gum_interceptor_begin_transaction (g_interceptor);

  hook_export (module, "tss_sdk_decryptpacket", &cfg_decrypt);  /* out = 明文 */
  hook_export (module, "tss_sdk_encryptpacket", &cfg_encrypt);  /* in  = 明文 */

  hook_export (module, "tss_sdk_set_token", &cfg_set_token);
  hook_export (module, "tss_sdk_setuserinfo", &cfg_setuser);
  hook_export (module, "tss_sdk_gen_session_data", &cfg_gensess);

  for (gsize i = 0; i < G_N_ELEMENTS (AES_SETKEY_CANDIDATES); i++)
  {
    gsize off = AES_SETKEY_CANDIDATES[i];
    gpointer addr = GSIZE_TO_POINTER (base + off);
    GumAttachOptions options = { 0, };
    LOGI ("[+] hook AES setkey @0x%zx", off);
    options.listener_function_data = &cfg_aes;
    gum_interceptor_attach (g_interceptor, addr, g_listener, &options);
  }

  gum_interceptor_end_transaction (g_interceptor);

  LOGI ("[*] hooks installed.");
}

/* ------------------------------------------------------------------ */
/* waitForModule：若模块已加载直接装 hook，否则 hook dlopen 等待         */
/* ------------------------------------------------------------------ */
static HookConfig cfg_dlopen_watch = { HOOK_DLOPEN_WATCH, "dlopen" };

static void
wait_for_module (void)
{
  GumModule * mod = gum_process_find_module_by_name (TARGET_MODULE);
  gpointer ip;
  GumAttachOptions options = { 0, };

  if (mod != NULL)
  {
    install_hooks (mod);
    g_object_unref (mod);
    return;
  }

  ip = GSIZE_TO_POINTER (
      gum_module_find_global_export_by_name ("android_dlopen_ext"));
  if (ip == NULL)
    ip = GSIZE_TO_POINTER (gum_module_find_global_export_by_name ("dlopen"));

  if (ip == NULL)
  {
    LOGI ("[!] dlopen export not found, cannot wait for %s", TARGET_MODULE);
    return;
  }

  gum_interceptor_begin_transaction (g_interceptor);
  options.listener_function_data = &cfg_dlopen_watch;
  gum_interceptor_attach (g_interceptor, ip, g_listener, &options);
  gum_interceptor_end_transaction (g_interceptor);
}

/* ------------------------------------------------------------------ */
/* 入口：注入后自动执行                                                 */
/* ------------------------------------------------------------------ */
__attribute__((constructor))
static void
tersafe_hook_entry (void)
{
  gum_init_embedded ();

  g_interceptor = gum_interceptor_obtain ();
  g_listener = gum_make_call_listener (on_enter, on_leave, NULL, NULL);

  wait_for_module ();
}
