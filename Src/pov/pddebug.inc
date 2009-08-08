
#include "colors.inc"

light_source { <-10, 25, -30> color White*2 }

cylinder { <0,0,0>, <100,0,0>, .1 pigment { color Red } }
cylinder { <0,0,0>, <0,100,0>, .1 pigment { color Green } }
cylinder { <0,0,0>, <0,0,100>, .1 pigment { color Blue } }
cylinder { <10,0,0>, <10,100,0>, .1 pigment { color rgb<.5, .5, .5> } }
cylinder { <0,15,0>, <100,15,0>, .1 pigment { color rgb<.5, .5, .5> } }
