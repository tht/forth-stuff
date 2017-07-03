PB12 constant A-MOTOR
PB13 constant A-MIN
PB14 constant A-MAX
PA11 constant B-MOTOR
PA12 constant B-MIN
PA15 constant B-MAX

: a-off A-MOTOR ioc! ; : a-on  A-MOTOR ios! ;
: b-off B-MOTOR ioc! ; : b-on  B-MOTOR ios! ;

: a-init ( - ) OMODE-PP A-MOTOR io-mode!  IMODE-PULL A-MIN io-mode! A-MIN ios! IMODE-PULL A-MAX io-mode! A-MAX ios! a-off ;
: b-init ( - ) OMODE-PP B-MOTOR io-mode!  IMODE-PULL B-MIN io-mode! B-MIN ios! IMODE-PULL B-MAX io-mode! B-MAX ios! b-off ;

: a-max@ ( - f ) A-MAX io@ not ; : a-min@ ( - f ) A-MIN io@ not ;
: b-max@ ( - f ) B-MAX io@ not ; : b-min@ ( - f ) B-MIN io@ not ;

: a-safe-to-run? ( - f ) a-min@ a-max@ not and ;
: b-safe-to-run? ( - f ) b-min@ b-max@ not and ;

: a-run-while-ok ( - ) begin a-safe-to-run? while a-on 1000 ms repeat a-off ;
: b-run-while-ok ( - ) begin b-safe-to-run? while b-on 1000 ms repeat b-off ;

task: a-run
: a-run& ( -- )
  a-run activate
  begin
    a-min@ if
      a-run-while-ok
    then
    1000 ms
  again ;

task: b-run
: b-run& ( -- )
  b-run activate
  begin
    b-min@ if
      b-run-while-ok
    then
    1000 ms
  again ;

: ab-start ( -- )
  a-init b-init
  a-run& b-run&
  multitask
;
