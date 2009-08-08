#include "pdglobal.inc"

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

    material { PDMaterial }
}

/*
sphere { <11, 19, 0>, .5   pigment { color Cyan } }
sphere { <9, 14.5, 0>, .5  pigment { color Cyan } }
sphere { <6.9, 10.5, 0>, .5  pigment { color Cyan } }
sphere { <5.7, 6, 0>, .5   pigment { color Cyan } }
sphere { <5.2, 1, 0>, .5   pigment { color Cyan } }
sphere { <5, -5, 0>, .5    pigment { color Cyan } }
* */
