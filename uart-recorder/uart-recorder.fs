: init ( - ) 
  init
  uart-irq-init
  9600 uart-baud
;

\ Push next uart key on stack or -1 after 5s timeout
: uart-irq-key-tmout ( - u )
  millis
  begin dup millis swap - 5000 > uart-ring ring# 0<> or until
  drop
  uart-ring ring# 0<> if
    uart-ring ring>
  else
    -1
  then
;

\ Some variables and a buffer to store temporary data
0 variable start-ts \ millis when recording started
0 variable last-ts  \ millis of last recorded byte
0 variable prev-ts  \ millis of previos packet published
0 variable buf-ts   \ millis when current packet started
0 variable buf-i    \ index of first free position
64 buffer: buf

\ Returns true if there is still space availabel in the buffer
: buf-free? ( - flag )
  buf-i @ 64 <
;

: buf-add ( byte - )
  buf buf-i @ + c!       \ store data byte
  buf-i @ 1+ buf-i !     \ calc and store ne buffer position
;

: buf-clear ( - )
  64 0 do
    0 buf i + c!
  loop
  0 buf-i !
;


: buf. ( - )
  64 0 do
    i 8 mod 0= if CR then
    buf i + c@ h.2 ."  "
  loop
;

\ Output buffer and clear it
: buf-dump ( - )
  buf-ts @ prev-ts @ - ( ts-offset )
  begin dup 1023 > while
    1023 6 lshift 0 or h.4 CR
    1023 -
  repeat
  6 lshift buf-i @ or h.4 \ shift time-offset and OR with size - output as 16bit
  buf-i @ 0<> if
    buf-i @ 0 do
      buf i + c@ h.2
    loop
  then CR
  0 buf-i !
  buf-ts @ prev-ts !
;

: output-data ( byte - )
  millis ( byte millis )
  start-ts @ 0 = if \ check if first byte
    dup start-ts ! dup prev-ts ! dup buf-ts ! ( byte millis )
    swap buf-add ( millis )
  else
    dup last-ts @ - 5 > if \ more than 5ms passed since last packet
      buf-dump
      dup buf-ts !
      swap buf-add ( millis )
    else
      swap buf-add ( millis )
    then
  then
  last-ts !
;

: clear-uart-buf ( - )
  begin
    uart-ring ring# 0<> while
    uart-ring ring>
  repeat
;

: uart-dumpsong
  CR
  clear-uart-buf
  0 start-ts !
  begin
    uart-irq-key-tmout
    dup
    -1 <> while
    output-data
  repeat
  buf-dump
  drop \ there was still a -1 on the stack left
;

: uart-dump begin uart-irq-key h.2 again ;
