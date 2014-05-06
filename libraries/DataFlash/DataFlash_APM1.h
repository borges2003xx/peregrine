/* ************************************************************ */
/* DataFlash_APM1 Log library                                 */
/* ************************************************************ */
#ifndef __DATAFLASH_APM1_H__
#define __DATAFLASH_APM1_H__

#include <AP_HAL.h>
#include "DataFlash.h"

class DataFlash_APM1 : public DataFlash_Class
{
private:
    //Methods
    uint8_t           BufferRead (uint8_t BufferNum, uint16_t IntPageAdr);
    void                    BufferWrite (uint8_t BufferNum, uint16_t IntPageAdr, uint8_t Data);
    void                    BufferToPage (uint8_t BufferNum, uint16_t PageAdr, uint8_t wait);
    void                    PageToBuffer(uint8_t BufferNum, uint16_t PageAdr);
    void                    WaitReady();
    uint8_t           ReadStatusReg();
    uint8_t           ReadStatus();
    uint16_t                PageSize();
    void                    PageErase (uint16_t PageAdr);
    void                    BlockErase (uint16_t BlockAdr);
    void                    ChipErase();
    
    AP_HAL::SPIDeviceDriver *_spi;
    AP_HAL::Semaphore *_spi_sem;
public:

    DataFlash_APM1() {}
    void        Init();
    void        ReadManufacturerID();
    bool        CardInserted();
};

#endif
