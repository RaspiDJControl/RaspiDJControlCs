namespace RaspiDJControl
{
    enum MessageType : byte
    {
        JOG_FORWARD = 0x01,
        JOG_BACKWARD = 0x02,
        SWITCH_PFL = 0x03,
        PLAYPAUSE_1 = 0x04,
        PLAYPAUSE_2 = 0x05,
        SYNC_1 = 0x06,
        SYNC_2 = 0x07,
        CROSSFADER = 0x14
    }
}