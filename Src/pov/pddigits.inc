
#macro PDdigit()
    // All the digits assume that the camera position is this
    camera { location <5, 7.5, -20> look_at <5, 7.5, 0> }

    // All the digits use the same set of light sources
    #declare xx = 0;
    #while (xx < 10)
        #declare yy = 0;
        #while (yy < 10)
            light_source { <-30 + 2*xx + 2*yy, 20 + 2*xx - 2*yy, -30> color <1, 1, 1>/50 }
            #declare yy = yy + 1;
        #end
        #declare xx = xx + 1;
    #end
#end

#macro PDdigitDebug()
    camera { location <5, 7.5, -20> look_at <5, 7.5, 0> }
    light_source { <-10, 25, -30> color <1, 1, 1>*2 }
    cylinder { <0,0,0>, <100,0,0>, .1 pigment { color <1, 0, 0> } }
    cylinder { <0,0,0>, <0,100,0>, .1 pigment { color <0, 1, 0> } }
    cylinder { <0,0,0>, <0,0,100>, .1 pigment { color <0, 0, 1> } }
    cylinder { <10,0,0>, <10,100,0>, .1 pigment { color rgb<.5, .5, .5> } }
    cylinder { <0,15,0>, <100,15,0>, .1 pigment { color rgb<.5, .5, .5> } }
#end

#declare PDdigit0 =
    sphere_sweep {
        b_spline
        13
        <8.5, 12, 0>, .7
        <8.5, 14.6, 0>, .7
        <2.8, 14.3, 0>, .5
        <.7, 10.1, 0>, .5
        <.5, 4, 0>, .5
        <3, .5, 0>, .5
        <7.1, .7, 0>, .7
        <9.2, 3.5, 0>, .7
        <9.5, 7.7, 0>, .5
        <8.7, 11.8, -.2>, .5
        <6, 13.5, -.2>, .5
        <2.8, 12.3, -.4>, .3
        <1.8, 9, -.6>, .1
        tolerance .02
    }

#declare PDdigit1 =
    merge {
        sphere_sweep {
            b_spline
            5
            <7.4, 17, 0>, .1
            <6, 14, 0>, .3
            <4.6, 11, 0>, .5
            <2.1, 9, 0>, .7
            <-.4, 7, 0>, .7
            tolerance .1
        }
        sphere_sweep {
            b_spline
            6
            <7.1, 18.5, 0>, 1
            <6.6, 14, 0>, 1
            <6.1, 10, 0>, .7
            <5.7, 5.5, 0>, .5
            <5.5, .7, 0>, .3
            <5.5, -4.1, 0>, .3
            tolerance .1
        }
        sphere_sweep {
            b_spline
            5
            <1.5, 0, 0>, .7
            <3.5, .4, 0>, .7
            <5.5, .7, 0>, .7
            <7.5, .7, 0>, .7
            <9.5, .4, 0>, .7
            tolerance .1
        }
    }

#declare PDdigit2 =
    merge {
        sphere_sweep {
            b_spline
            11
            <3, 8, 0>, 1,
            <.7, 10, 0>, 1,
            <.7, 12.7, 0>, .7,
            <3, 14.3, 0>, .7,
            <7, 14.3, 0>, .7,
            <9.3, 12.5, 0>, .7,
            <9.3, 9, 0>, .7,
            <5, 6.5, 0>, .7,
            <.4, 3.5, 0>, .4,
            <.4, .8, 0>, .4,
            <.4, -1.9, 0>, .4
            tolerance .1
        }
        sphere_sweep {
            b_spline
            6
            <-1.4, .2, 0>, .8,
            <.8, .8, 0>, .8,
            <3, 1.4, 0>, .7,
            <7, 1.4, 0>, .7,
            <10, .4, 0>, .3,
            <13, -.6, 0>, .1
            tolerance .1
        }
    }

#declare PDdigit3 =
    merge {
        sphere_sweep {
            b_spline
            10
            <3, 8.4, 0>, 1,
            <.7, 10.3, 0>, 1,
            <.7, 12.7, 0>, .7,
            <3, 14.3, 0>, .7,
            <6.7, 14.3, 0>, .7,
            <9, 12.7, 0>, .7,
            <9, 9, 0>, .7,
            <6, 7.75, 0>, .5,
            <5, 7.75, 0>, .3,
            <4, 7.75, 0>, .2
            tolerance .1
        }
        sphere_sweep {
            b_spline
            10
            <4, 7.65, 0>, .2,
            <5, 7.65, 0>, .3,
            <6, 8, 0>, .5,
            <9.3, 6, 0>, .7,
            <9.3, 2.5, 0>, .8,
            <7, .7, 0>, .8,
            <3, .7, 0>, .7,
            <.4, 2, 0>, .5,
            <.4, 5, 0>, .3,
            <3, 7, 0>, .2
            tolerance .1
        }
    }

#declare PDdigit4 =
    merge {
        sphere_sweep {
            b_spline
            5
            <3.3, 17, 0>, .9,
            <3.3, 15, 0>, .9,
            <2, 8, 0>, .8,
            <.7, 5, 0>, .7,
            <-.6, 2, 0>, .7
            tolerance .1
        }
        sphere_sweep {
            b_spline
            6
            <-1.3, 4.5, 0>, .7,
            <.7, 5, 0>, .7,
            <2.7, 5.5, -.11>, .7,
            <7, 5.5, -.11>, .6,
            <9.65, 5, 0>, .4,
            <12, 4.5, 0>, .3
            tolerance .1
        }
        sphere_sweep {
            b_spline
            5
            <6.6, 12, 0>, .7,
            <6.2, 10, 0>, .7,
            <6, 5.4, 0>, .7,
            <5.7, .5, 0>, .7,
            <5.7, -4.4, 0>, .7
            tolerance .1
        }
    }

#declare PDdigit5 =
    merge {
        sphere_sweep {
            b_spline
            6
            <11, 15.5, 0>, 1,
            <8.5, 14.5, 0>, 1,
            <6, 13.5, 0>, .7,
            <3.5, 13.5, 0>, .7,
            <2, 14.05, -.05>, .65,
            <.5, 14.6, -.1>, .6
            tolerance .1
        }
        sphere_sweep {
            b_spline
            4
            <2.5, 20, 0>, .7,
            <2, 14, 0>, .7,
            <1.5, 8, 0>, .7,
            <1, 2, 0>, .7
            tolerance .1
        }
        sphere_sweep {
            b_spline
            11
            <-1, 6.35, -.6>, .2,
            <1.6, 7.8, -.3>, .3,
            <4.2, 9, 0>, .5,
            <7, 9, 0>, .7,
            <9.5, 6.8, 0>, .7,
            <9.3, 2.8, 0>, .8,
            <6.7, .7, 0>, .8,
            <3, .7, 0>, .7,
            <.6, 2, 0>, .5,
            <.4, 5, 0>, .3,
            <2, 7, 0>, .2
            tolerance .1
        }
    }

#declare PDdigit6 =
    sphere_sweep {
        b_spline
        15
        <9.7, 12.4, 0>, 1
        <8.8, 13.4, 0>, 1
        <6.9, 14.4, 0>, .7
        <3.2, 13.8, 0>, .7
        <.7, 9.7, 0>, .6
        <.7, 3.5, 0>, .6
        <3, .7, 0>, .7
        <6.55, .7, 0>, .7
        <9.2, 3, 0>, .8
        <9.3, 7.3, 0>, .75
        <7.3, 9.8, 0>, .7
        <4, 9.8, 0>, .7
        <1.85, 7.5, 0>, .5
        <1.85, 5, 0>, .3
        <2.6, 2.7, 0>, .1
        tolerance .1
    }

#declare PDdigit7 =
    merge {
        sphere_sweep {
            b_spline
            6
            <-1, 16.5, 0>, 1
            <1, 14.5, 0>, 1
            <3, 13.5, 0>, .7
            <7, 13.5, 0>, .5
            <9, 14.5, 0>, .3
            <11, 16.5, 0>, .1
            tolerance .1
        }
        sphere_sweep {
            b_spline
            6
            <11, 19, 0>, .3
            <9, 14.5, 0>, .3
            <6.9, 10.5, 0>, .5
            <5.7, 6, 0>, .7
            <5.2, 1, 0>, 1
            <5, -5, 0>, 1
            tolerance .1
        }
    }

#declare PDdigit8 =
    sphere_sweep {
        b_spline
        12
        <8.2, 12, 0>, .7
        <8.2, 14.6, 0>, .7
        <2, 14.3, 0>, .5
        <2, 8.2, 0>, .5
        <9, 6.8, 0>, .5
        <9, .5, 0>, .7
        <1, .3, 0>, .5
        <1, 6.8, 0>, .7
        <8.5, 8.2, 0>, .7
        <8.5, 13.3, -.2>, .5
        <2.7, 13.3, -.4>, .3
        <3, 9, -.6>, .1
        tolerance .02
    }

#declare PDdigit9 =
    merge {
        sphere_sweep {
            b_spline
            10
            <10.4, 11.7, 0>, .7
            <8.3, 13, 0>, .7
            <6.2, 14.4, 0>, .7
            <2.8, 14.4, 0>, .8
            <1, 12.4, 0>, .8
            <.5, 8.5, 0>, .7
            <2.8, 6, 0>, .5
            <6, 6, 0>, .5
            <8, 7, 0>, .3
            <10, 8, 0>, .3
            tolerance .02
        }
        sphere_sweep {
            b_spline
            6
            <9, 18, 0>, 1
            <8.5, 14, 0>, 1
            <8.3, 10, 0>, .7
            <7.9, 5, 0>, .7
            <7.8, .5, 0>, .5
            <7.8, -4, 0>, .5
            tolerance .02
        }
    }

#declare PDDigitsMaterial = material {
    texture {
        pigment { color rgbt<1.2, .8, 0, .125> }
        normal { wrinkles .1 scale 1 }
        finish { phong .5 }
    }
}

#declare PDDigitsDebugMaterial = material {
    texture {
        pigment { color rgbt<1.2, .8, 0, .5> }
        normal { wrinkles .1 scale 1 }
        finish { phong .5 }
    }
}

#macro PDWhite()
    box { <-100, -100, 1.5>, <100, 100, 1.6> pigment { rgb<2, 2, 2> } }
#end
