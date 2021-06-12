# SPI_Example
SPI read/write example

1. Wiring
```
NUCLEOF722ZE            EVAL-AD4111/12SDZ

PA7(SPI1_MOSI)          MOSI
PA6(SPI1_MISO)          MISO
PA5(SPI1_SCK)           SCLK
PA4(SPI1_NSS)           /CS
GND                     GND
5V                      5V
GND                     VCOM
5V                      VIN0
```

2. F722ZE_SPI
- NUCLEOF722ZE board
- STM32CubeMX 6.2.1
- STM32CubeIDE 1.6.1
- AD4111 register read/write

3. SPIWpfApp
- ad4111_reg_ch0_bipolar_voltage.json : exported register file from AD411x Eval+ program
- .NET Core WPF Application
- .NET 5.0

3.1. Voltage read(CH0, bipolar voltage)
3.1.1. Load default AD4111 register values
- def_ad4111_ch0_bipolar_voltage.txt

3.1.2. Open COM port
- Input COM port name and press Open button

3.1.3. Write AD4111 registers(CH0 bipolar voltage)
- Select CH0 in the datagrid and click Write button.<br/>
  Select GPIOCON in the datagrid and click Write button.<br/>
       :<br/>
  Select IFMODE in the datagrid and click Write button.<br/>

3.1.4. Read voltage value
- Select Data in the datagrid and click Read button.
