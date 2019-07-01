### A sample one line calculator

#### Supported number format
* hex : ```(0x)[\\da-fA-F]+``` e.g. 0xFF12
* bin : ```(0b)[01]+|[01]+b``` e.g. 0b10110, 1011b
* double,integer e.g. 123.456, 4321
#### Supported constant
* ```pi``` : 3.1415926535...
* ```constE``` : 2.7182818284... (mathematical constant)
#### Supported operator
* Arithmetic operators
  * ```\+ \- \* \/ %```
* Bitwise logical operator
  * ```| & ^ << >>```
    * \^:xor
    * << >> : float value is support but not
* Trigonometric functions
  * ```cosh tanh sinh acos atan asin cos tan sin```
* Others
  * ```log```
    * base 2
  * ```exp sqrt```
  * ```pow```
    * e.g. ```2pow2``` = 4
  * ```int```
    * convert double to integer for integer only operator

#### Screenshot

![Screenshot](https://raw.githubusercontent.com/Harpseal/OneLineCalculator/master/OneLineCalculator/Resource/ScreenShots/Screenshot_01.png)