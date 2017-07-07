## Data Format
Describes how recorded data is stored.

The data is stored as timestamp/data pairs. Every block starts with a 2 Byte header containing the time difference since the start of the last header and the number of bytes in this time period.

### Header

* `TDIFF`: 10 Bit, unsigned (0-1023): Time offset (in ms) since the start of the last header timestamp. May be `0` if this data follows immediately after the last data pair.
* `SIZE`: 6 Bit, unsigned (0-63): Number of Bytes in this time period. May be `0` if this data pair is only used as placeholder.

### Data

After this header follows the data. The exact same number of bytes as communicated in the header field `SIZE`. It has to be sent as fast as the baud rate allows.

## EOF Marker

The last *Header* always is `0x0000` without any data. This value does not occur in a normal use-case.