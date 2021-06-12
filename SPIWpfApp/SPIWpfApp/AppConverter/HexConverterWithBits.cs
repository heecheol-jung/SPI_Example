using RegisterCore.Net.Models;
using System;
using System.Globalization;
using System.Windows.Data;

namespace SPIWpfApp.AppConverter
{
    // duckduckgo : wpf converter parameter
    // http://www.shujaat.net/2011/02/wpf-binding-converter-parameter.html
    public class HexConverterWithBits : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            RegisterTemplate regTemplate = (values[1] == null) ? null : values[1] as RegisterTemplate;
            
            if (values[0] is long)
            {
                long input = (long)values[0];
                return GetStringOutput((ulong)input, regTemplate);
            }
            else if (values[0] is ulong)
            {
                ulong input = (ulong)values[0];
                return GetStringOutput(input, regTemplate);
            }
            else if (values[0] is int)
            {
                int input = (int)values[0];
                return GetStringOutput((ulong)input, regTemplate);
            }
            else if (values[0] is int)
            {
                int input = (int)values[0];
                return GetStringOutput((ulong)input, regTemplate);
            }
            else if (values[0] is short)
            {
                short input = (short)values[0];
                return GetStringOutput((ulong)input, regTemplate);
            }
            else if (values[0] is ushort)
            {
                ushort input = (ushort)values[0];
                return GetStringOutput((ulong)input, regTemplate);
            }
            else if (values[0] is sbyte)
            {
                sbyte input = (sbyte)values[0];
                return GetStringOutput((ulong)input, regTemplate);
            }
            else if (values[0] is byte)
            {
                byte input = (byte)values[0];
                return GetStringOutput((ulong)input, regTemplate);
            }
            else
            {
                return string.Empty;
            }
        }

        private string GetStringOutput(ulong inValue, RegisterTemplate regTemplate)
        {
            if (regTemplate != null)
            {
                if (regTemplate.Bits <= 8)
                {
                    return string.Format("{0:X2}", (byte)inValue);
                }
                else if (regTemplate.Bits <= 16)
                {
                    return string.Format("{0:X4}", (ushort)inValue);
                }
                else if (regTemplate.Bits <= 32)
                {
                    return string.Format("{0:X8}", (uint)inValue);
                }
                else if (regTemplate.Bits <= 64)
                {
                    return string.Format("{0:X16}", (ulong)inValue);
                }
                else
                {
                    return string.Format("{0:X}", inValue);
                }
            }
            else
            {
                return string.Format("{0:X}", inValue);
            }   
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
