namespace RaspiDJControl
{
    class MidiMessage
    {
        public byte Function { get; set; }
        public byte Note { get; set; }
        public byte Value { get; set; }
        public bool Input { get; set; }
        
        public static implicit operator byte[](MidiMessage message)
        {
            return new[] { message.Function, message.Note, message.Value };
        }

        public static explicit operator MidiMessage(byte[] bytes)  // explicit byte to digit conversion operator
        {
            return new MidiMessage
            {
                Function = bytes[0],
                Note = bytes[1],
                Value = bytes[2]
            };
        }
    }
}
