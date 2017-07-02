PA3 constant LED-A \ Port PA4 nennen wir LED-A
PA5 constant LED-B \ Port PA5 ist LED-B

task: a-blink      \ Speicher reservieren für Hintergrundprozess
: a-blink& ( - )   \ Hintergrundprozess definieren
  a-blink activate
  OMODE-PP LED-A io-mode! \ Pin von LED-A in Push-Pull Modus setzen
  begin
    LED-A iox!     \ Pin LED-A umschalten
    250 ms         \ 250 ms warten - dahinter steckt das Multitasking
  again ;          \ Endlosschleife von begin zu again

task: b-blink      \ Speicher reservieren für Hintergrundprozess
: b-blink& ( - )   \ Hintergrundprozess definieren
  b-blink activate
  OMODE-PP LED-B io-mode! \ Pin von LED-B in Push-Pull Modus setzen
  begin
    LED-B iox!     \ Pin LED-B umschalten
    300 ms         \ 300 ms warten - dahinter steckt das Multitasking
  again ;          \ Endlosschleife von begin zu again

a-blink&           \ Hintergrundprozess für LED-A starten
b-blink&           \ und für LED-B
multitask          \ Multitasker aktivieren - LEDs blinken!
