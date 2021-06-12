#include <stdio.h>
#include <string.h>
#include "fw_app.h"
#include "ad411x_regs.h"

extern TIM_HandleTypeDef htim2;

FL_DECLARE_DATA fw_app_t g_app;

#if FW_APP_PARSER_CALLBACK == 1
static void on_message_parsed(const void* parser_handle, void* context);
#endif

FL_DECLARE(void) fw_app_init(void)
{
  memset(&g_app, 0, sizeof(g_app));

  // Serial port for message communication.
  g_app.proto_mgr.uart_handle = &huart3;
  g_app.proto_mgr.parser_handle.on_parsed_callback = on_message_parsed;
  g_app.proto_mgr.parser_handle.context = (void*)&g_app;

  g_app.ad4111.spi_desc = &hspi1;
}

FL_DECLARE(void) fw_app_hw_init(void)
{
  ad717x_init_param ad4111_init_param;
  // TODO : Device id setting(DIP switch, flash storage, ...).
  g_app.device_id = 1;

  // GPIO output pin for debugging.
  HAL_GPIO_WritePin(DBG_OUT1_GPIO_Port, DBG_OUT1_Pin, GPIO_PIN_RESET);
  HAL_GPIO_WritePin(DBG_OUT2_GPIO_Port, DBG_OUT2_Pin, GPIO_PIN_RESET);

  // Message receive in interrupt mode.
  FW_APP_UART_RCV_IT(g_app.proto_mgr.uart_handle, g_app.proto_mgr.rx_buf, 1);

  ad4111_init_param.regs = ad4111_regs;
  ad4111_init_param.num_regs = sizeof(ad4111_regs) / sizeof(ad4111_regs[0]);
  ad4111_init_param.spi_init.spi_timeout = 100;
  AD717X_Init(&g_app.ad4111, ad4111_init_param);
}

FL_DECLARE(void) fw_app_systick(void)
{
  g_app.tick++;
  // Do some work every 1 second.
  if (g_app.tick >= FW_APP_ONE_SEC_INTERVAL)
  {
    // LED1 toggle.
    HAL_GPIO_TogglePin(LD1_GPIO_Port, LD1_Pin);
    g_app.tick = 0;
  }
}

#if FW_APP_PARSER_CALLBACK == 1
static void on_message_parsed(const void* parser_handle, void* context)
{
  fl_txt_msg_parser_t*    txt_parser = (fl_txt_msg_parser_t*)parser_handle;
  fw_app_proto_manager_t* proto_mgr = &((fw_app_t*)context)->proto_mgr;

  // Ignore the parsed message.
  if (txt_parser->device_id != ((fw_app_t*)context)->device_id)
  {
    return;
  }

  switch (txt_parser->msg_id)
  {
  case FL_MSG_ID_READ_HW_VERSION:
  {
    proto_mgr->out_length = sprintf((char*)proto_mgr->out_buf, "%s %ld,%d,%d.%d.%d%c",
        fl_txt_msg_get_message_name(txt_parser->msg_id),
        txt_parser->device_id,
        FL_OK,
        FW_APP_HW_MAJOR, FW_APP_HW_MINOR, FW_APP_HW_REVISION,
        FL_TXT_MSG_TAIL);
    break;
  }

  case FL_MSG_ID_READ_FW_VERSION:
  {
    proto_mgr->out_length = sprintf((char*)proto_mgr->out_buf, "%s %ld,%d,%d.%d.%d%c",
        fl_txt_msg_get_message_name(txt_parser->msg_id),
        txt_parser->device_id,
        FL_OK,
        FW_APP_FW_MAJOR, FW_APP_FW_MINOR, FW_APP_FW_REVISION,
        FL_TXT_MSG_TAIL);
    break;
  }

  case FL_MSG_ID_READ_WRITE_SPI:
  {
    if (txt_parser->arg_count >= 3)
    {
      fl_spi_read_t* spi_read = (fl_spi_read_t*)&(proto_mgr->parser_handle.payload);
      if (spi_read->rw_mode == FL_MSG_SPI_READ)
      {
        if (AD717X_ReadRegister(&g_app.ad4111, (uint8_t)spi_read->reg_addr) == 0)
        {
          ad717x_st_reg* preg = AD717X_GetReg(&g_app.ad4111, (uint8_t)spi_read->reg_addr);
          if (preg != NULL)
          {
            proto_mgr->out_length = sprintf((char*)proto_mgr->out_buf, "%s %ld,%d,%d,%d,%d,%ld%c",
                            fl_txt_msg_get_message_name(txt_parser->msg_id),
                            txt_parser->device_id,
                            FL_OK,
                            spi_read->rw_mode,
                            spi_read->spi_num,
                            spi_read->reg_addr,
                            preg->value,
                            FL_TXT_MSG_TAIL);
          }
        }
      }
      else if (spi_read->rw_mode == FL_MSG_SPI_WRITE)
      {
        if (txt_parser->arg_count == 4)
        {
          fl_spi_write_t* spi_write = (fl_spi_write_t*)&(proto_mgr->parser_handle.payload);
          ad717x_st_reg* preg = AD717X_GetReg(&g_app.ad4111, (uint8_t)spi_read->reg_addr);
          if (preg != NULL)
          {
            preg->value = (int32_t)spi_write->reg_value;
            if (AD717X_WriteRegister(&g_app.ad4111, (uint8_t)spi_read->reg_addr) == 0)
            {
              proto_mgr->out_length = sprintf((char*)proto_mgr->out_buf, "%s %ld,%d%c",
                                        fl_txt_msg_get_message_name(txt_parser->msg_id),
                                        txt_parser->device_id,
                                        FL_OK,
                                        FL_TXT_MSG_TAIL);
            }
          }
        }
      }
    }

    if (proto_mgr->out_length == 0)
    {
      proto_mgr->out_length = sprintf((char*)proto_mgr->out_buf, "%s %ld,%d%c",
                fl_txt_msg_get_message_name(txt_parser->msg_id),
                txt_parser->device_id,
                FL_ERROR,
                FL_TXT_MSG_TAIL);
    }
    break;
  }
  }

  if (proto_mgr->out_length > 0)
  {
    HAL_UART_Transmit(proto_mgr->uart_handle, proto_mgr->out_buf, proto_mgr->out_length, FW_APP_PROTO_TX_TIMEOUT);
  }
  proto_mgr->out_length = 0;
}
#endif

