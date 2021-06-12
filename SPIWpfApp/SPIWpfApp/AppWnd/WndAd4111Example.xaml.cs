using Fl.Net.Message;
using Microsoft.Win32;
using RegisterCore.Net;
using RegisterCore.Net.Models;
using RegisterSqlite.Net;
using SPIWpfApp.AppControl;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SPIWpfApp.AppWnd
{
    /// <summary>
    /// Interaction logic for WndAd4111Example.xaml
    /// </summary>
    public partial class WndAd4111Example : Window
    {
        long _currentChipId = -1;
        int _bitFieldBits = 0;
        ObservableCollection<Register> _registers = new ObservableCollection<Register>();
        UcRegisterBitUsage _regBitUsage = new UcRegisterBitUsage();
        SPIManager _spiMgr = new SPIManager();
        Brush _normalColor;

        public WndAd4111Example()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _normalColor = TbBitFieldValue.Background;

            using (var ctx = new RegisterContext())
            {
                var chip = ctx.Chips.Where(c => c.Name == "AD4111").SingleOrDefault();
                if (chip != null)
                {
                    _currentChipId = chip.Id;
                }
            }

            BtnRegisterRead.IsEnabled = false;
            BtnRegisterWrite.IsEnabled = false;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (_spiMgr.IsStarted())
            {
                _spiMgr.Stop();
            }
        }

        private void BtnRegisterValueLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                TbFIleName.Text = openFileDialog.SafeFileName;
                var regValues = AppUtil.ReadRegisterValuesFromFile(openFileDialog.FileName);

                UpdateRegisterDataGrid(regValues);
            }
        }

        private void BtnRegisterValueExport_Click(object sender, RoutedEventArgs e)
        {
            if ((_registers == null) ||
                (_registers.Count <= 0))
            {
                MessageBox.Show("No register values.");
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    AppUtil.SaveRegisterValues(saveFileDialog.FileName, _registers);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        private void DgRegister_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgRegister.SelectedIndex < 0)
            {
                DgRegisterBitField.ItemsSource = null;
                BitFieldPlaceHolder.Content = null;
                TbRegisterName.Text = "";
                TbRegisterValue.Text = "";
                return;
            }

            Register reg = (Register)DgRegister.SelectedItem;
            if (reg != null)
            {
                DgRegisterBitField.ItemsSource = reg.BitFields;

                _bitFieldBits = reg.Bits;
                _regBitUsage.UpdateRegBitUsage(_bitFieldBits, reg.BitFields.ToList());
                BitFieldPlaceHolder.Content = _regBitUsage;

                TbRegisterName.Text = reg.Name;
                TbRegisterValue.Text = $"{reg.Value:X4}";
            }
            else
            {
                BitFieldPlaceHolder.Content = null;
                TbRegisterName.Text = "";
                TbRegisterValue.Text = "";
            }
        }

        private void BtnAddRegisterValue_Click(object sender, RoutedEventArgs e)
        {
            List<RegisterTemplate> regTemplates = null;
            using (var ctx = new RegisterContext())
            {
                regTemplates = ctx.RegisterTemplates.Where(r => r.ChipId == _currentChipId).ToList();
            }

            if (_registers?.Count > 0)
            {
                foreach (var regValue in _registers)
                {
                    var regTemplate = regTemplates.Where(r => r.Id == regValue.Id).SingleOrDefault();
                    if (regTemplate != null)
                    {
                        regTemplates.Remove(regTemplate);
                    }
                }
            }

            WndRegisterValueAdd wndRegValueAdd = new WndRegisterValueAdd(regTemplates);
            if (wndRegValueAdd.ShowDialog() == true)
            {
                _registers.Add(wndRegValueAdd.RegisterValue);
                if (DgRegister.ItemsSource == null)
                {
                    DgRegister.ItemsSource = _registers;
                }
            }
        }

        private void BtnRemoveRegisterValue_Click(object sender, RoutedEventArgs e)
        {
            if (DgRegister.SelectedIndex < 0)
            {
                MessageBox.Show("No register selected.");
                return;
            }

            Register reg = (Register)DgRegister.SelectedItem;
            _registers.Remove(reg);
        }

        private void DgRegisterBitField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgRegisterBitField.SelectedIndex < 0)
            {
                TbBitFieldName.Text = "";
                TbBitFieldValue.Text = "";
                return;
            }

            BitField bf = (BitField)DgRegisterBitField.SelectedItem;
            if (bf != null)
            {
                TbBitFieldName.Text = $"{bf.Name}";
                TbBitFieldValue.Text = $"{bf.Value:X4}";
            }
        }

        private void BtnBitFieldValueApply_Click(object sender, RoutedEventArgs e)
        {
            int selectedRegIndex = DgRegister.SelectedIndex;
            int selectedBfIndex = DgRegisterBitField.SelectedIndex;
            if (selectedBfIndex < 0)
            {
                MessageBox.Show("No selected bit field.");
                return;
            }

            if (string.IsNullOrEmpty(TbBitFieldValue.Text) == true)
            {
                MessageBox.Show("No bit field value.");
                return;
            }

            Register reg = (Register)DgRegister.SelectedItem;
            BitField bf = (BitField)DgRegisterBitField.SelectedItem;
            if (ulong.TryParse(TbBitFieldValue.Text, System.Globalization.NumberStyles.HexNumber, null, out ulong bfValue) == true)
            {
                if (bfValue > GeneralUtil.GetBitFieldMaxValue(bf))
                {
                    MessageBox.Show("Check maximum value.");
                    return;
                }
                bf.Value = bfValue;

                UInt64 calculatedVal = GeneralUtil.GetRegisterValueFromBitFields(reg.BitFields);

                reg.Value = calculatedVal;

                DgRegister.ItemsSource = null;
                DgRegister.ItemsSource = _registers;
                DgRegister.SelectedIndex = selectedRegIndex;
                DgRegisterBitField.ItemsSource = null;
                DgRegisterBitField.ItemsSource = reg.BitFields;
                DgRegisterBitField.SelectedIndex = selectedBfIndex;
            }
        }

        private void BtnRegisterValueApply_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = DgRegister.SelectedIndex;
            if (selectedIndex < 0)
            {
                MessageBox.Show("No selected register.");
                return;
            }

            if (string.IsNullOrEmpty(TbRegisterValue.Text) == true)
            {
                MessageBox.Show("No register value.");
                return;
            }

            Register reg = (Register)DgRegister.SelectedItem;
            if (ulong.TryParse(TbRegisterValue.Text, System.Globalization.NumberStyles.HexNumber, null, out ulong regValue) == true)
            {
                reg.Value = regValue;

                foreach (var bf in reg.BitFields)
                {
                    var val = GeneralUtil.GetBitFieldValue(reg.Value, bf);

                    bf.Value = val;
                }

                DgRegister.ItemsSource = null;
                DgRegister.ItemsSource = _registers;
                DgRegister.SelectedIndex = selectedIndex;
                DgRegisterBitField.ItemsSource = null;
                DgRegisterBitField.ItemsSource = reg.BitFields;
            }
        }

        private void BtnComPortOpenClose_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TbComPortName.Text))
            {
                MessageBox.Show("Port name empty.");
                return;
            }

            try
            {
                string strOpenClose = (string)BtnComPortOpenClose.Content;
                switch (strOpenClose)
                {
                    case "Open":
                        _spiMgr.Start(TbComPortName.Text);
                        break;

                    case "Close":
                        _spiMgr.Stop();
                        break;
                }

                if (_spiMgr.IsStarted() == true)
                {
                    BtnComPortOpenClose.Content = "Close";
                    BtnRegisterRead.IsEnabled = true;
                    BtnRegisterWrite.IsEnabled = true;
                }
                else
                {
                    BtnComPortOpenClose.Content = "Open";
                    BtnRegisterRead.IsEnabled = false;
                    BtnRegisterWrite.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void BtnRegisterRead_Click(object sender, RoutedEventArgs e)
        {
            if (DgRegister.SelectedIndex < 0)
            {
                MessageBox.Show("No register selected.");
                return;
            }

            Register reg = (Register)DgRegister.SelectedItem;
            IFlMessage response = _spiMgr.ReadRegister((ushort)reg.Address);
            LbHistory.Items.Insert(0, "ReadRegister command sent");

            string strResp = "No response";
            if (response != null)
            {
                if ((response.Arguments?.Count > 0) &&
                    (response.Arguments.Count == 6))
                {
                    strResp = "Invalid response(register value error)";
                    if (reg.Bits <= 8)
                    {
                        if (byte.TryParse((string)response.Arguments[5], out byte regValue))
                        {
                            strResp = string.Format("Resp : 0x{0:X4}, 0x{1:X2}", reg.Address, regValue);
                        }
                    }
                    else if (reg.Bits <= 16)
                    {
                        if (ushort.TryParse((string)response.Arguments[5], out ushort regValue))
                        {
                            strResp = string.Format("Resp : 0x{0:X4}, 0x{1:X4}", reg.Address, regValue);
                        }
                    }
                    else if (reg.Bits <= 32)
                    {
                        if (uint.TryParse((string)response.Arguments[5], out uint regValue))
                        {
                            

                           
                            strResp = string.Format("Resp : 0x{0:X4}, 0x{1:X8}", reg.Address, regValue);

                            if (reg.Address == 0x04)
                            {
                                // Bipolar voltage
                                double _measuredValue = (((int)regValue / AppConstant.BIPOLAR_CONST1) - 1.0) * AppConstant.VREF * 10.0;

                                // Unipolar voltage
                                //double _measuredValue = regValue * AppConstant.VREF * 10.0 / AppConstant.UNIPOLAR_CONST1;
                                strResp += $"\r\nVoltage : {_measuredValue}";
                            }
                        }
                    }
                }
                else
                {
                    strResp = "Invalid response(argument error)";
                }
            }

            LbHistory.Items.Insert(0, strResp);
        }

        private void BtnRegisterWrite_Click(object sender, RoutedEventArgs e)
        {
            if (DgRegister.SelectedIndex < 0)
            {
                MessageBox.Show("No register selected.");
                return;
            }
            if (string.IsNullOrEmpty(TbRegisterValue.Text))
            {
                MessageBox.Show("Empty register value");
                return;
            }
            if (!uint.TryParse(TbRegisterValue.Text, NumberStyles.HexNumber, null, out uint regValue))
            {
                MessageBox.Show("Invalid register value");
                return;
            }

            Register reg = (Register)DgRegister.SelectedItem;
            IFlMessage response = _spiMgr.WriteRegister((ushort)reg.Address, regValue);
            LbHistory.Items.Insert(0, "WriteRegister command sent");

            string strResp = "No response";
            if (response != null)
            {
                if ((response.Arguments?.Count > 0) &&
                    (response.Arguments?.Count == 2))
                {
                    strResp = "Register write OK";
                    if ((string)response.Arguments[1] == "1")
                    {
                        strResp = "Register write fail";
                    }
                }
                else
                {
                    strResp = "Invalid response(argument error)";
                }
            }

            LbHistory.Items.Insert(0, strResp);
        }

        private void BtnHistoryClear_Click(object sender, RoutedEventArgs e)
        {
            LbHistory.Items.Clear();
        }

        private void TbBitFieldValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DgRegisterBitField.SelectedIndex >= 0)
            {
                BitField bf = (BitField)DgRegisterBitField.SelectedItem;
                AppUtil.ValidateBitFieldInput(TbBitFieldValue, bf, _normalColor);
            }
            else
            {
                TbBitFieldValue.Background = _normalColor;
            }
        }

        private void TbRegisterValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DgRegister.SelectedIndex >= 0)
            {
                Register reg = (Register)DgRegister.SelectedItem;
                AppUtil.ValidateRegisterInput(TbRegisterValue, reg, _normalColor);
            }
            else
            {
                TbRegisterValue.Background = _normalColor;
            }
        }

        private void UpdateRegisterDataGrid(List<(ulong address, uint value)> regValues)
        {
            _registers.Clear();

            using (var ctx = new RegisterContext())
            {
                foreach (var item in regValues)
                {
                    var reg = ctx.RegisterTemplates.Where(r => r.ChipId == _currentChipId && r.Address == item.address).FirstOrDefault();

                    if (reg != null)
                    {
                        var bitFields = new ObservableCollection<BitFieldTemplate>(ctx.BitFieldTemplates.Where(bf => bf.RegisterTemplateId == reg.Id).OrderBy(bf => bf.Offset).ToList());
                        reg.BitFields = bitFields;
                        Register regValue = new Register(reg);
                        regValue.Value = item.value;

                        foreach (var bf in reg.BitFields)
                        {
                            BitField b = new BitField(bf);
                            var val = GeneralUtil.GetBitFieldValue(item.value, bf);

                            b.Value = val;
                            regValue.BitFields.Add(b);
                        }
                        _registers.Add(regValue);
                    }
                }

                DgRegister.ItemsSource = _registers;
            }
        }
    }
}
