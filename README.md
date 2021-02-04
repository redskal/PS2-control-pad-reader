# PS2 Controller Readout
###### _Display input from a PlayStation2 control pad_

This project is the start of my prototyping phase for a terrestrial drone I was
designing for use in airsoft and milsim's.

_TL;DR - I've reverse-engineered the information from the control pad and
displayed it as useful data to build a control centre._

The idea was to design a drone from an old RC car and use it to deliver Mark5
flashbang grenades to the opposition without risking 'lives'. It was a project
I started back in January 2020; when the pandemic hit the need for this drone
was delayed and so it has been shelved for the time being.

My plan is to use the input taken from a PS2 control pad and transmit that from
an old EeePC netbook to the drone. Eventually it will have camera feeds, but I
need airsoft sites to open back up to consider moving forward with this project
at this point.

###### Code

The code is a little rough; this iteration was more about reverse-engineering
the data sent from the controller and being able to display it as useful values.

It's designed to run under Mono on Linux. Developing this way for peripherals
is a dream. Windows is far more convulted to just get a PoC up and running.

This project uses the HidSharp library [github.com/IntergatedCircuits/HidSharp](https://github.com/IntergatedCircuits/HidSharp)