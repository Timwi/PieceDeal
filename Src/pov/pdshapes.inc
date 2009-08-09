
// camera { location <15, 7.5, -5> look_at <5, 7.5, 0> }
camera { location <0, 0, -13> look_at <0, 0, 0> angle 15 }

global_settings {
    assumed_gamma 1
    max_trace_level 30
    radiosity {
        count 50
        error_bound 2
        recursion_limit 2
        nearest_count 8
        brightness 1
        normal on
    }
}

#declare PDgreen = color rgbt<.05, 1, .2, 0.4>;
#declare PDred = color rgbt<1, 0, .1, 0.4>;
#declare PDblue = color rgbt<.25, .5, 1, 0.4>;
#declare PDyellow = color rgbt<1, 1, .15, 0.4>;

#macro PDShapeLight()
    light_source {
        <-12, 12, -15>
        color <2, 2, 2>
    }
#end

#macro PDShapeMaterial(clr)
    material {
        texture {
            pigment { color clr }
            normal { bumps 0.15 scale 1 translate 1*x+1*y }
            finish { phong .5 }
        }
    }
#end

#declare cone_sph = .05;
#declare PDcone = merge {
    cone {
        <0, 1 - 4*cone_sph/sqrt(5), 0>, 2*cone_sph/sqrt(5)
        <0, -1 + cone_sph + cone_sph/sqrt(5), 0>, 1 - (cone_sph + cone_sph/sqrt(5))/2
    }
    sphere {
        <0, 1 - 5*cone_sph/sqrt(5), 0>, cone_sph
    }
    torus {
        1 - cone_sph/2 - 5/2*cone_sph/sqrt(5),
        cone_sph
        translate (-1 + cone_sph)*y
    }
    cylinder {
        <0, -1, 0>, <0, -1 + 2*cone_sph, 0>, 1 - cone_sph/2 - 5/2*cone_sph/sqrt(5)
    }
}

#declare cross_sph = .4;
#declare PDcross = blob {
    cylinder { <-1, 0, 0>, <1, 0, 0>, cross_sph, 2 }
    cylinder { <0, -1, 0>, <0, 1, 0>, cross_sph, 2 }
    cylinder { <0, 0, -1>, <0, 0, 1>, cross_sph, 2 }
    rotate 35*y
    rotate -30*x
    rotate -10*z
}

#declare PDcube = superellipsoid { <.3, .3> scale .8 rotate 35*y rotate -30*x }

#declare PDcircle = sphere { <0, 0, 0>, 1 scale .4*z }

#macro PDWhite()
    background { color rgb<1, 1, 1> }
#end
