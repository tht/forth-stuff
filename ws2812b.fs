\ ws2812b.fs
\ Some basic methods for driving WS2812B LEDs using DMA on SPI2.
\ Only tested on *Blue Pill*, timing depends on 72MHz CPU clock.
\ Output is on MOSI2 (PB15)
\ 
\ Needs spi2.fs


\ --------------------------------------------------
\  Register adresses - some are already defined
\ --------------------------------------------------

\ $40020000 constant DMA1
\   DMA1 $00 + constant DMA1-ISR
\   DMA1 $04 + constant DMA1-IFCR
    DMA1 $58 + constant DMA1-CCR5
    DMA1 $5C + constant DMA1-CNDTR5
    DMA1 $60 + constant DMA1-CPAR5
    DMA1 $64 + constant DMA1-CMAR5


\ --------------------------------------------------
\  Configuration
\ --------------------------------------------------

\ maximum number of LEDs (using 9 Bytes of memory each!)
[ifndef] MAX-LEDS 60 constant MAX-LEDS [then]


\ --------------------------------------------------
\  Internal Helpers
\ --------------------------------------------------

\ Changes a flag to %110 (if true) or %100 (otherwise)
: bit2triplett ( f - u ) if %110 else %100 then ;

\ Iterates over all bits from input-byte and replaces them by bit2triplett output
\ 1 byte becomes 3 bytes
: byte2triplett ( u  - u ) 0 0 7 do 3 lshift over i bit and bit2triplett or -1 +loop nip ;

\ Write one color-byte in WS2812B format to 3 bytes in memory
: writetriplett ( color base_addr index - )
  3 * + >r \ calculate addr base and store on return stack
  byte2triplett 
           dup $ff and r@ 2 + c! \ output last byte
  8 rshift dup $ff and r@ 1+  c! \ output second byte
  8 rshift     $ff and r>     c! \ output first byte and clear return stack
;


\ Calculate number of bytes needed for a given number of LEDs
: led2bytes ( u - u ) 2 + 9 * ; \ one empty at beginning and end, 3 bytes per color times 3 colors for every led

\ in-memory buffer -> 9 bytes for every LED
MAX-LEDS led2bytes buffer: strip
strip MAX-LEDS led2bytes $00 fill


\ --------------------------------------------------
\  External API
\ --------------------------------------------------

\ Sets color of an LED (r, g, b are 0-255 / index is 0-based)
: setpixel ( r g b index - )
  1+ 9 * strip + >r
  r@ 2 writetriplett
  r@ 0 writetriplett
  r> 1 writetriplett
;

: led-clear ( - )
  MAX-LEDS 0 do
    $00 $00 $00 i setpixel
  loop
;

: led-show ( )
  0 bit DMA1-CCR5 bic!   \ Make sure channel is disabled
  $0F00 DMA1-IFCR !      \ Clear all the interrupt flags for ch5
  MAX-LEDS led2bytes DMA1-CNDTR5 !       \ 9 Bytes to transfer
  0 bit DMA1-CCR5 bis!   \ Make sure channel is enabled
;

: led-init ( - )
  0 bit RCC-AHBENR bis!  \ DMA1EN clock enable

  led-clear              \ clear buffer

  0 bit DMA1-CCR5 bic!   \ Make sure channel is disabled
  $0F00 DMA1-IFCR !      \ Clear all the interrupt flags for ch5

  MAX-LEDS led2bytes DMA1-CNDTR5 ! \ number of bytes to transfer
  strip DMA1-CMAR5 !     \ read from strip memory
  SPI2-DR DMA1-CPAR5 !   \ write to SPI2

  %10   12 lshift        \ high priority
  %0000  8 lshift or     \ 8 bit transfers
  %10    6 lshift or     \ auto-increment on memory address only
  %0     5 lshift or     \ one-shot
  %1     4 lshift or     \ read from memory
  %000   1 lshift or     \ no interrupts
  %1              or     \ enable channel
    DMA1-CCR5 !

  \ now init spi2 and configure for DMA TX
  OMODE-AF-PP SCLK2 io-mode!
  OMODE-AF-PP MOSI2 io-mode!
  14 bit RCC-APB1ENR bis!  \ set SPI2EN
  %1100000001011100 SPI2-CR1 !  \ clk/8, i.e. 4.5 MHz, master
  \ SPI2-SR @ drop         \ appears to be needed to avoid hang in some cases
  1 bit SPI2-CR2 bis!    \ enable DMA Tx

  led-show
;


\ --------------------------------------------------
\  Testing
\ --------------------------------------------------

\ Output memory buffer in internal format
: led. ( - )
  MAX-LEDS 1+ 1 do
    CR i h.2 ." :"
    9 0 do ."  " strip j 9 * + i + c@ h.2 loop
  loop
;

\ returns one of the colors for use in an animation
: colorwheel ( i - u u u )
  5 mod case
    0 of $ff $ff $ff endof
    1 of $ff $00 $00 endof
    2 of $00 $ff $00 endof
    3 of $00 $00 $ff endof
    4 of $00 $00 $00 endof
  endcase ;

\ output some demo data ( input is offset for animation)
: demodata ( i )
  MAX-LEDS 0 do
    dup i + colorwheel i setpixel
  loop led-show
;

: ringanimate ( - )
  0 100 do
    i demodata 200 ms
  loop
;
