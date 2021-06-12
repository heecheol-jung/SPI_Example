﻿using System.Collections.Generic;
using System.Windows.Controls;

namespace SPIWpfApp.AppControl
{
    /// <summary>
    /// Interaction logic for Uc24Bit.xaml
    /// </summary>
    public partial class Uc24Bit : UserControl
    {
        private List<UcBitFieldBit> _bits = new List<UcBitFieldBit>();
        
        public List<UcBitFieldBit> Bits { get => _bits; }

        public Uc24Bit()
        {
            InitializeComponent();

            _bits.Add(Bit23);
            _bits.Add(Bit22);
            _bits.Add(Bit21);
            _bits.Add(Bit20);
            _bits.Add(Bit19);
            _bits.Add(Bit18);
            _bits.Add(Bit17);
            _bits.Add(Bit16);

            _bits.Add(Bit15);
            _bits.Add(Bit14);
            _bits.Add(Bit13);
            _bits.Add(Bit12);
            _bits.Add(Bit11);
            _bits.Add(Bit10);
            _bits.Add(Bit9);
            _bits.Add(Bit8);
            _bits.Add(Bit7);
            _bits.Add(Bit6);
            _bits.Add(Bit5);
            _bits.Add(Bit4);
            _bits.Add(Bit3);
            _bits.Add(Bit2);
            _bits.Add(Bit1);
            _bits.Add(Bit0);
        }
    }
}
