; 自定义 NSIS 安装向导：完成页提供「创建桌面快捷方式」复选框（默认勾选，可取消）

!macro customFinishPage
  !define MUI_FINISHPAGE_SHOWREADME ""
  !define MUI_FINISHPAGE_SHOWREADME_TEXT "创建桌面快捷方式"
  !define MUI_FINISHPAGE_SHOWREADME_FUNCTION CreateDesktopShortcutFn
  !insertmacro MUI_PAGE_FINISH
!macroend

; 该函数仅在安装器编译阶段被完成页引用，卸载器阶段需排除以免“未引用”告警
!ifndef BUILD_UNINSTALLER
  Function CreateDesktopShortcutFn
    CreateShortcut "$DESKTOP\${PRODUCT_NAME}.lnk" "$INSTDIR\${APP_FILENAME}.exe"
  FunctionEnd
!endif

; 卸载时清理手动创建的桌面快捷方式
!macro customUnInstall
  Delete "$DESKTOP\${PRODUCT_NAME}.lnk"
!macroend
