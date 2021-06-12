using RegisterCore.Net;
using RegisterCore.Net.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;

namespace SPIWpfApp
{
    public static class AppUtil
    {
        public static List<(ulong address, uint value)> ReadRegisterValuesFromFile(string fileName)
        {
            string line;
            char[] seperator = new char[] { ',' };
            List<(ulong address, uint value)> regValues = new List<(ulong address, uint value)>();

            try
            {
                StreamReader sr = new StreamReader(fileName);
                while ((line = sr.ReadLine()) != null)
                {
                    string[] args = line.Split(seperator);
                    if (args?.Length == 2)
                    {
                        if (ulong.TryParse(args[0], NumberStyles.HexNumber, null, out ulong addr) == true)
                        {
                            if (uint.TryParse(args[1], NumberStyles.HexNumber, null, out uint val) == true)
                            {
                                regValues.Add((addr, val));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }

            return regValues;
        }

        public static void SaveRegisterValues(string fileName, ObservableCollection<Register> registers)
        {
            StreamWriter sw = new StreamWriter(fileName);

            foreach (var reg in registers)
            {
                if (reg.Bits <= 8)
                {
                    sw.WriteLine($"{reg.Address:X8},{reg.Value:X2}");
                }
                else if (reg.Bits <= 16)
                {
                    sw.WriteLine($"{reg.Address:X8},{reg.Value:X4}");
                }
                else if (reg.Bits <= 32)
                {
                    sw.WriteLine($"{reg.Address:X8},{reg.Value:X8}");
                }
                else
                {
                    sw.WriteLine($"{reg.Address:X8},{reg.Value:X}");
                }
            }

            sw.Close();
        }

        public static void ValidateBitFieldInput(TextBox tb, BitField bf, Brush normalColor)
        {
            bool error = false;
            string strHex = tb.Text;

            if (!string.IsNullOrEmpty(strHex))
            {
                if (!UInt64.TryParse(strHex, System.Globalization.NumberStyles.HexNumber, null, out ulong bfValue))
                {
                    error = true;
                }
                else
                {
                    if (bfValue > GeneralUtil.GetMaxValueOfBits(bf.Bits))
                    {
                        error = true;
                    }
                }
            }

            if (error)
            {
                tb.Background = Brushes.Red;
            }
            else
            {
                if (tb.Background != normalColor)
                {
                    tb.Background = normalColor;
                }
            }
        }

        public static void ValidateRegisterInput(TextBox tb, Register register, Brush normalColor)
        {
            bool error = false;
            string strHex = tb.Text;

            if (!string.IsNullOrEmpty(strHex))
            {
                if (!UInt64.TryParse(strHex, System.Globalization.NumberStyles.HexNumber, null, out ulong bfValue))
                {
                    error = true;
                }
                else
                {
                    if (bfValue > GeneralUtil.GetMaxValueOfBits(register.Bits))
                    {
                        error = true;
                    }
                }
            }

            if (error)
            {
                tb.Background = Brushes.Red;
            }
            else
            {
                if (tb.Background != normalColor)
                {
                    tb.Background = normalColor;
                }
            }
        }
    }
}
