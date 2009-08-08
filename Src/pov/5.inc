
#include "pdglobal.inc"

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

    material { PDMaterial }
}

/*
sphere { <-1, 6.35, -.6>, .5 pigment { color Cyan } }
sphere { <1.6, 7.8, -.3>, .5 pigment { color Cyan } }
sphere { <4.2, 9, 0>, .5     pigment { color Cyan } }
sphere { <7, 9, 0>, .5       pigment { color Cyan } }
sphere { <9.5, 6.8, 0>, .5   pigment { color Cyan } }
sphere { <9.3, 2.8, 0>, .5   pigment { color Cyan } }
sphere { <6.7, .7, 0>, .5    pigment { color Cyan } }
sphere { <3, .7, 0>, .5      pigment { color Cyan } }
sphere { <.6, 2, 0>, .5      pigment { color Cyan } }
sphere { <.4, 5, 0>, .5      pigment { color Cyan } }
sphere { <2, 7, 0>, .5       pigment { color Cyan } }
*/
