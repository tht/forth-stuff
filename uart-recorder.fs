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

0 variable startm

: timediff ( - n )
  startm @ 0 = if
    millis startm !
    0
  else
    millis startm @ -
  then
;

0 variable lastm

: output-data ( n - )
  startm @ 0 = if
    timediff lastm ! CR 0 . h.2
  else
    timediff dup lastm @ - 5 > if
      dup CR . swap h.2
    else
      swap h.2
    then
    lastm !
  then
;

: clear-uart-buf ( - )
  begin
    uart-ring ring# 0= while
    uart-ring ring>
  repeat
;

: uart-dumpsong
  clear-uart-buf
  0 startm !
  begin
    uart-irq-key-tmout
    dup
    -1 <> while
    output-data
  repeat
;

: uart-dump begin uart-irq-key h.2 again ;
