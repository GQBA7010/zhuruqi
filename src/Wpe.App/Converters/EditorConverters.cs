using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using WPELibrary.Lib;

namespace Wpe.App.Converters
{
    /// <summary>bool -> Visibility（true = Visible）。</summary>
    public sealed class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (value is bool b && b) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => value is System.Windows.Visibility v && v == System.Windows.Visibility.Visible;
    }

    /// <summary>bool -> Visibility（true = Collapsed）。</summary>
    public sealed class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (value is bool b && b) ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => value is System.Windows.Visibility v && v != System.Windows.Visibility.Visible;
    }

    /// <summary>封包类型 -> 本地化名称（复用原生内核映射，保证 1:1）。</summary>
    public sealed class PacketTypeToNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Socket_Cache.SocketPacket.PacketType pt)
                return Socket_Cache.SocketPacket.GetName_ByPacketType(pt);
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }

    /// <summary>封包类型 -> 颜色徽标（发送=蓝，接收=绿），矢量白主题标识。</summary>
    public sealed class PacketTypeToBrushConverter : IValueConverter
    {
        private static readonly SolidColorBrush Send = new SolidColorBrush(Color.FromRgb(0x25, 0x63, 0xEB));
        private static readonly SolidColorBrush Recv = new SolidColorBrush(Color.FromRgb(0x16, 0xA3, 0x4A));

        static PacketTypeToBrushConverter()
        {
            Send.Freeze();
            Recv.Freeze();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Socket_Cache.SocketPacket.PacketType pt)
            {
                switch (pt)
                {
                    case Socket_Cache.SocketPacket.PacketType.WS1_Send:
                    case Socket_Cache.SocketPacket.PacketType.WS2_Send:
                    case Socket_Cache.SocketPacket.PacketType.WS1_SendTo:
                    case Socket_Cache.SocketPacket.PacketType.WS2_SendTo:
                    case Socket_Cache.SocketPacket.PacketType.WSASend:
                    case Socket_Cache.SocketPacket.PacketType.WSASendTo:
                        return Send;
                    default:
                        return Recv;
                }
            }
            return Recv;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }

    /// <summary>封包类型 -> 方向标记（↑ 发送 / ↓ 接收）。</summary>
    public sealed class PacketTypeToArrowConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Socket_Cache.SocketPacket.PacketType pt)
            {
                switch (pt)
                {
                    case Socket_Cache.SocketPacket.PacketType.WS1_Send:
                    case Socket_Cache.SocketPacket.PacketType.WS2_Send:
                    case Socket_Cache.SocketPacket.PacketType.WS1_SendTo:
                    case Socket_Cache.SocketPacket.PacketType.WS2_SendTo:
                    case Socket_Cache.SocketPacket.PacketType.WSASend:
                    case Socket_Cache.SocketPacket.PacketType.WSASendTo:
                        return "\u2191";
                    default:
                        return "\u2193";
                }
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}
