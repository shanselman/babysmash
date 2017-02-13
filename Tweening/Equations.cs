namespace Tweener
{
    using System;

    /// <summary>
    /// Equations
    /// Main equations for the Tweener class
    /// author		Michael Cameron
    /// version		1.0.1
    /// Updated for Silverlight 2.0 Beta 1
    /// </summary>

    /*
    Disclaimer for Robert Penner's Easing Equations license:

    TERMS OF USE - EASING EQUATIONS

    Open source under the BSD License.

    Copyright © 2001 Robert Penner
    All rights reserved.

    Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

        * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
        * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
        * Neither the name of the author nor the names of contributors may be used to endorse or promote products derived from this software without specific prior written permission.

    THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
    */

    public static class Equations
    {
        // ==================================================================================================================================
        // TWEENING EQUATIONS functions -----------------------------------------------------------------------------------------------------
        // (the original equations are Robert Penner's work as mentioned on the disclaimer)

        /**
		 * Easing equation function for a simple linear tweening, with no easing.
		 *
		 * @param t		Current time (in seconds).
		 * @param b		Starting value.
		 * @param c		Change needed in value.
		 * @param d		Expected easing duration (in seconds).
		 * @return		The correct value.
		 */
        public static double EaseNone(params double[] args)
        {
            double t; double b; double c; double d;
            t = args[0]; b = args[1]; c = args[2]; d = args[3];
            return c * t / d + b;
        }

        /**
		 * Easing equation function for a quadratic (t^2) easing in: accelerating from zero velocity.
		 *
		 * @param t		Current time (in seconds).
		 * @param b		Starting value.
		 * @param c		Change needed in value.
		 * @param d		Expected easing duration (in seconds).
		 * @return		The correct value.
		 */
        public static double EaseInQuad(params double[] args)
        {
            double t; double b; double c; double d;
            t = args[0]; b = args[1]; c = args[2]; d = args[3];
            return c * (t /= d) * t + b;
        }

        /**
         * Easing equation function for a quadratic (t^2) easing out: decelerating to zero velocity.
         *
         * @param t		Current time (in seconds).
         * @param b		Starting value.
         * @param c		Change needed in value.
         * @param d		Expected easing duration (in seconds).
         * @return		The correct value.
         */
        public static double EaseOutQuad(params double[] args)
        {
            double t; double b; double c; double d;
            t = args[0]; b = args[1]; c = args[2]; d = args[3];
            return -c * (t /= d) * (t - 2) + b;
        }

        /**
         * Easing equation function for a quadratic (t^2) easing in/out: acceleration until halfway, then deceleration.
         *
         * @param t		Current time (in seconds).
         * @param b		Starting value.
         * @param c		Change needed in value.
         * @param d		Expected easing duration (in seconds).
         * @return		The correct value.
         */
        public static double EaseInOutQuad(params double[] args)
        {
            double t; double b; double c; double d;
            t = args[0]; b = args[1]; c = args[2]; d = args[3];

            if ((t /= d / 2) < 1) return c / 2 * t * t + b;
            return -c / 2 * ((--t) * (t - 2) - 1) + b;
        }

        /**
         * Easing equation function for a quadratic (t^2) easing out/in: deceleration until halfway, then acceleration.
         *
         * @param t		Current time (in seconds).
         * @param b		Starting value.
         * @param c		Change needed in value.
         * @param d		Expected easing duration (in seconds).
         * @return		The correct value.
         */
        public static double EaseOutInQuad(params double[] args)
        {
            double t; double b; double c; double d;
            t = args[0]; b = args[1]; c = args[2]; d = args[3];

            if (t < d / 2) return EaseOutQuad(t * 2, b, c / 2, d);
            return EaseInQuad((t * 2) - d, b + c / 2, c / 2, d);
        }

        /**
         * Easing equation function for a cubic (t^3) easing in: accelerating from zero velocity.
         *
         * @param t		Current time (in seconds).
         * @param b		Starting value.
         * @param c		Change needed in value.
         * @param d		Expected easing duration (in seconds).
         * @return		The correct value.
         */
        public static double EaseInCubic(params double[] args)
        {
            double t; double b; double c; double d;
            t = args[0]; b = args[1]; c = args[2]; d = args[3];

            return c * (t /= d) * t * t + b;
        }

        /**
         * Easing equation function for a cubic (t^3) easing out: decelerating from zero velocity.
         *
         * @param t		Current time (in seconds).
         * @param b		Starting value.
         * @param c		Change needed in value.
         * @param d		Expected easing duration (in seconds).
         * @return		The correct value.
         */
        public static double EaseOutCubic(params double[] args)
        {
            double t; double b; double c; double d;
            t = args[0]; b = args[1]; c = args[2]; d = args[3];

            return c * ((t = t / d - 1) * t * t + 1) + b;
        }

        /**
         * Easing equation function for a cubic (t^3) easing in/out: acceleration until halfway, then deceleration.
         *
         * @param t		Current time (in seconds).
         * @param b		Starting value.
         * @param c		Change needed in value.
         * @param d		Expected easing duration (in seconds).
         * @return		The correct value.
         */
        public static double EaseInOutCubic(params double[] args)
        {
            double t; double b; double c; double d;
            t = args[0]; b = args[1]; c = args[2]; d = args[3];

            if ((t /= d / 2) < 1) return c / 2 * t * t * t + b;
            return c / 2 * ((t -= 2) * t * t + 2) + b;
        }

        /**
         * Easing equation function for a cubic (t^3) easing out/in: deceleration until halfway, then acceleration.
         *
         * @param t		Current time (in seconds).
         * @param b		Starting value.
         * @param c		Change needed in value.
         * @param d		Expected easing duration (in seconds).
         * @return		The correct value.
         */
        public static double EaseOutInCubic(params double[] args)
        {
            double t; double b; double c; double d;
            t = args[0]; b = args[1]; c = args[2]; d = args[3];

            if (t < d / 2) return EaseOutCubic(t * 2, b, c / 2, d);
            return EaseInCubic((t * 2) - d, b + c / 2, c / 2, d);
        }

        /**
         * Easing equation function for a quartic (t^4) easing in: accelerating from zero velocity.
         *
         * @param t		Current time (in seconds).
         * @param b		Starting value.
         * @param c		Change needed in value.
         * @param d		Expected easing duration (in seconds).
         * @return		The correct value.
         */
        public static double EaseInQuart(params double[] args)
        {
            double t; double b; double c; double d;
            t = args[0]; b = args[1]; c = args[2]; d = args[3];

            return c * (t /= d) * t * t * t + b;
        }

        /**
         * Easing equation function for a quartic (t^4) easing out: decelerating from zero velocity.
         *
         * @param t		Current time (in seconds).
         * @param b		Starting value.
         * @param c		Change needed in value.
         * @param d		Expected easing duration (in seconds).
         * @return		The correct value.
         */
        public static double EaseOutQuart(params double[] args)
        {
            double t; double b; double c; double d;
            t = args[0]; b = args[1]; c = args[2]; d = args[3];

            return -c * ((t = t / d - 1) * t * t * t - 1) + b;
        }

        /**
         * Easing equation function for a quartic (t^4) easing in/out: acceleration until halfway, then deceleration.
         *
         * @param t		Current time (in seconds).
         * @param b		Starting value.
         * @param c		Change needed in value.
         * @param d		Expected easing duration (in seconds).
         * @return		The correct value.
         */
        public static double EaseInOutQuart(params double[] args)
        {
            double t; double b; double c; double d;
            t = args[0]; b = args[1]; c = args[2]; d = args[3];

            if ((t /= d / 2) < 1) return c / 2 * t * t * t * t + b;
            return -c / 2 * ((t -= 2) * t * t * t - 2) + b;
        }

        /**
         * Easing equation function for a quartic (t^4) easing out/in: deceleration until halfway, then acceleration.
         *
         * @param t		Current time (in seconds).
         * @param b		Starting value.
         * @param c		Change needed in value.
         * @param d		Expected easing duration (in seconds).
         * @return		The correct value.
         */
        public static double EaseOutInQuart(params double[] args)
        {
            double t; double b; double c; double d;
            t = args[0]; b = args[1]; c = args[2]; d = args[3];

            if (t < d / 2) return EaseOutQuart(t * 2, b, c / 2, d);
            return EaseInQuart((t * 2) - d, b + c / 2, c / 2, d);
        }

        /**
         * Easing equation function for a quintic (t^5) easing in: accelerating from zero velocity.
         *
         * @param t		Current time (in seconds).
         * @param b		Starting value.
         * @param c		Change needed in value.
         * @param d		Expected easing duration (in seconds).
         * @return		The correct value.
         */
        public static double EaseInQuint(params double[] args)
        {
            double t; double b; double c; double d;
            t = args[0]; b = args[1]; c = args[2]; d = args[3];

            return c * (t /= d) * t * t * t * t + b;
        }

        /**
         * Easing equation function for a quintic (t^5) easing out: decelerating from zero velocity.
         *
         * @param t		Current time (in seconds).
         * @param b		Starting value.
         * @param c		Change needed in value.
         * @param d		Expected easing duration (in seconds).
         * @return		The correct value.
         */
        public static double EaseOutQuint(params double[] args)
        {
            double t; double b; double c; double d;
            t = args[0]; b = args[1]; c = args[2]; d = args[3];

            return c * ((t = t / d - 1) * t * t * t * t + 1) + b;
        }

        /**
         * Easing equation function for a quintic (t^5) easing in/out: acceleration until halfway, then deceleration.
         *
         * @param t		Current time (in seconds).
         * @param b		Starting value.
         * @param c		Change needed in value.
         * @param d		Expected easing duration (in seconds).
         * @return		The correct value.
         */
        public static double EaseInOutQuint(params double[] args)
        {
            double t; double b; double c; double d;
            t = args[0]; b = args[1]; c = args[2]; d = args[3];

            if ((t /= d / 2) < 1) return c / 2 * t * t * t * t * t + b;
            return c / 2 * ((t -= 2) * t * t * t * t + 2) + b;
        }

        /**
         * Easing equation function for a quintic (t^5) easing out/in: deceleration until halfway, then acceleration.
         *
         * @param t		Current time (in seconds).
         * @param b		Starting value.
         * @param c		Change needed in value.
         * @param d		Expected easing duration (in seconds).
         * @return		The correct value.
         */
        public static double EaseOutInQuint(params double[] args)
        {
            double t; double b; double c; double d;
            t = args[0]; b = args[1]; c = args[2]; d = args[3];

            if (t < d / 2) return EaseOutQuint(t * 2, b, c / 2, d);
            return EaseInQuint((t * 2) - d, b + c / 2, c / 2, d);
        }

        /**
         * Easing equation function for a sinusoidal (sin(t)) easing in: accelerating from zero velocity.
         *
         * @param t		Current time (in seconds).
         * @param b		Starting value.
         * @param c		Change needed in value.
         * @param d		Expected easing duration (in seconds).
         * @return		The correct value.
         */
        public static double EaseInSine(params double[] args)
        {
            double t; double b; double c; double d;
            t = args[0]; b = args[1]; c = args[2]; d = args[3];

            return -c * Math.Cos(t / d * (Math.PI / 2)) + c + b;
        }

        /**
         * Easing equation function for a sinusoidal (sin(t)) easing out: decelerating from zero velocity.
         *
         * @param t		Current time (in seconds).
         * @param b		Starting value.
         * @param c		Change needed in value.
         * @param d		Expected easing duration (in seconds).
         * @return		The correct value.
         */
        public static double EaseOutSine(params double[] args)
        {
            double t; double b; double c; double d;
            t = args[0]; b = args[1]; c = args[2]; d = args[3];

            return c * Math.Sin(t / d * (Math.PI / 2)) + b;
        }

        /**
         * Easing equation function for a sinusoidal (sin(t)) easing in/out: acceleration until halfway, then deceleration.
         *
         * @param t		Current time (in seconds).
         * @param b		Starting value.
         * @param c		Change needed in value.
         * @param d		Expected easing duration (in seconds).
         * @return		The correct value.
         */
        public static double EaseInOutSine(params double[] args)
        {
            double t; double b; double c; double d;
            t = args[0]; b = args[1]; c = args[2]; d = args[3];

            return -c / 2 * (Math.Cos(Math.PI * t / d) - 1) + b;
        }

        /**
         * Easing equation function for a sinusoidal (sin(t)) easing out/in: deceleration until halfway, then acceleration.
         *
         * @param t		Current time (in seconds).
         * @param b		Starting value.
         * @param c		Change needed in value.
         * @param d		Expected easing duration (in seconds).
         * @return		The correct value.
         */
        public static double EaseOutInSine(params double[] args)
        {
            double t; double b; double c; double d;
            t = args[0]; b = args[1]; c = args[2]; d = args[3];

            if (t < d / 2) return EaseOutSine(t * 2, b, c / 2, d);
            return EaseInSine((t * 2) - d, b + c / 2, c / 2, d);
        }

        /**
         * Easing equation function for an exponential (2^t) easing in: accelerating from zero velocity.
         *
         * @param t		Current time (in seconds).
         * @param b		Starting value.
         * @param c		Change needed in value.
         * @param d		Expected easing duration (in seconds).
         * @return		The correct value.
         */
        public static double EaseInExpo(params double[] args)
        {
            double t; double b; double c; double d;
            t = args[0]; b = args[1]; c = args[2]; d = args[3];

            return (t == 0) ? b : c * Math.Pow(2, 10 * (t / d - 1)) + b - c * 0.001;
        }

        /**
         * Easing equation function for an exponential (2^t) easing out: decelerating from zero velocity.
         *
         * @param t		Current time (in seconds).
         * @param b		Starting value.
         * @param c		Change needed in value.
         * @param d		Expected easing duration (in seconds).
         * @return		The correct value.
         */
        public static double EaseOutExpo(params double[] args)
        {
            double t; double b; double c; double d;
            t = args[0]; b = args[1]; c = args[2]; d = args[3];

            return (t == d) ? b + c : c * 1.001 * (-Math.Pow(2, -10 * t / d) + 1) + b;
        }

        /**
         * Easing equation function for an exponential (2^t) easing in/out: acceleration until halfway, then deceleration.
         *
         * @param t		Current time (in seconds).
         * @param b		Starting value.
         * @param c		Change needed in value.
         * @param d		Expected easing duration (in seconds).
         * @return		The correct value.
         */
        public static double EaseInOutExpo(params double[] args)
        {
            double t; double b; double c; double d;
            t = args[0]; b = args[1]; c = args[2]; d = args[3];

            if (t == 0) return b;
            if (t == d) return b + c;
            if ((t /= d / 2) < 1) return c / 2 * Math.Pow(2, 10 * (t - 1)) + b - c * 0.0005;
            return c / 2 * 1.0005 * (-Math.Pow(2, -10 * --t) + 2) + b;
        }

        /**
         * Easing equation function for an exponential (2^t) easing out/in: deceleration until halfway, then acceleration.
         *
         * @param t		Current time (in seconds).
         * @param b		Starting value.
         * @param c		Change needed in value.
         * @param d		Expected easing duration (in seconds).
         * @return		The correct value.
         */
        public static double EaseOutInExpo(params double[] args)
        {
            double t; double b; double c; double d;
            t = args[0]; b = args[1]; c = args[2]; d = args[3];

            if (t < d / 2) return EaseOutExpo(t * 2, b, c / 2, d);
            return EaseInExpo((t * 2) - d, b + c / 2, c / 2, d);
        }

        /**
         * Easing equation function for a circular (sqrt(1-t^2)) easing in: accelerating from zero velocity.
         *
         * @param t		Current time (in seconds).
         * @param b		Starting value.
         * @param c		Change needed in value.
         * @param d		Expected easing duration (in seconds).
         * @return		The correct value.
         */
        public static double EaseInCirc(params double[] args)
        {
            double t; double b; double c; double d;
            t = args[0]; b = args[1]; c = args[2]; d = args[3];

            return -c * (Math.Sqrt(1 - (t /= d) * t) - 1) + b;
        }

        /**
         * Easing equation function for a circular (sqrt(1-t^2)) easing out: decelerating from zero velocity.
         *
         * @param t		Current time (in seconds).
         * @param b		Starting value.
         * @param c		Change needed in value.
         * @param d		Expected easing duration (in seconds).
         * @return		The correct value.
         */
        public static double EaseOutCirc(params double[] args)
        {
            double t; double b; double c; double d;
            t = args[0]; b = args[1]; c = args[2]; d = args[3];

            return c * Math.Sqrt(1 - (t = t / d - 1) * t) + b;
        }

        /**
         * Easing equation function for a circular (sqrt(1-t^2)) easing in/out: acceleration until halfway, then deceleration.
         *
         * @param t		Current time (in seconds).
         * @param b		Starting value.
         * @param c		Change needed in value.
         * @param d		Expected easing duration (in seconds).
         * @return		The correct value.
         */
        public static double EaseInOutCirc(params double[] args)
        {
            double t; double b; double c; double d;
            t = args[0]; b = args[1]; c = args[2]; d = args[3];

            if ((t /= d / 2) < 1) return -c / 2 * (Math.Sqrt(1 - t * t) - 1) + b;
            return c / 2 * (Math.Sqrt(1 - (t -= 2) * t) + 1) + b;
        }

        /**
         * Easing equation function for a circular (sqrt(1-t^2)) easing out/in: deceleration until halfway, then acceleration.
         *
         * @param t		Current time (in seconds).
         * @param b		Starting value.
         * @param c		Change needed in value.
         * @param d		Expected easing duration (in seconds).
         * @return		The correct value.
         */
        public static double EaseOutInCirc(params double[] args)
        {
            double t; double b; double c; double d;
            t = args[0]; b = args[1]; c = args[2]; d = args[3];

            if (t < d / 2) return EaseOutCirc(t * 2, b, c / 2, d);
            return EaseInCirc((t * 2) - d, b + c / 2, c / 2, d);
        }

        /**
         * Easing equation function for an elastic (exponentially decaying sine wave) easing in: accelerating from zero velocity.
         *
         * @param t		Current time (in seconds).
         * @param b		Starting value.
         * @param c		Change needed in value.
         * @param d		Expected easing duration (in seconds).
         * @param a		Amplitude.
         * @param p		Period.
         * @return		The correct value.
         */
        //public static double EaseInElastic (double t, double b, double c, double d, double a, double p) {
        public static double EaseInElastic(params double[] args)
        {
            double t; double b; double c; double d; double a; double p;
            t = args[0]; b = args[1]; c = args[2]; d = args[3]; a = ((args.Length < 5) ? 0 : args[4]); p = ((args.Length < 6) ? 0 : args[5]);

            if (t == 0) return b; if ((t /= d) == 1) return b + c; if (p == 0) p = d * .3;
            double s;
            if (a == 0 || a < Math.Abs(c)) { a = c; s = p / 4; }
            else s = p / (2 * Math.PI) * Math.Asin(c / a);
            return -(a * Math.Pow(2, 10 * (t -= 1)) * Math.Sin((t * d - s) * (2 * Math.PI) / p)) + b;
        }

        /**
         * Easing equation function for an elastic (exponentially decaying sine wave) easing out: decelerating from zero velocity.
         *
         * @param t		Current time (in seconds).
         * @param b		Starting value.
         * @param c		Change needed in value.
         * @param d		Expected easing duration (in seconds).
         * @param a		Amplitude.
         * @param p		Period.
         * @return		The correct value.
         */
        public static double EaseOutElastic(params double[] args)
        {
            double t; double b; double c; double d; double a; double p;
            t = args[0]; b = args[1]; c = args[2]; d = args[3]; a = ((args.Length < 5) ? 0 : args[4]); p = ((args.Length < 6) ? 0 : args[5]);

            if (t == 0) return b; if ((t /= d) == 1) return b + c; if (p == 0) p = d * .3;
            double s;
            if (a == 0 || a < Math.Abs(c)) { a = c; s = p / 4; }
            else s = p / (2 * Math.PI) * Math.Asin(c / a);
            return (a * Math.Pow(2, -10 * t) * Math.Sin((t * d - s) * (2 * Math.PI) / p) + c + b);
        }

        /**
         * Easing equation function for an elastic (exponentially decaying sine wave) easing in/out: acceleration until halfway, then deceleration.
         *
         * @param t		Current time (in seconds).
         * @param b		Starting value.
         * @param c		Change needed in value.
         * @param d		Expected easing duration (in seconds).
         * @param a		Amplitude.
         * @param p		Period.
         * @return		The correct value.
         */
        public static double EaseInOutElastic(params double[] args)
        {
            double t; double b; double c; double d; double a; double p;
            t = args[0]; b = args[1]; c = args[2]; d = args[3]; a = ((args.Length < 5) ? 0 : args[4]); p = ((args.Length < 6) ? 0 : args[5]);

            if (t == 0) return b; if ((t /= d / 2) == 2) return b + c; if (p == 0) p = d * (.3 * 1.5);
            double s;
            if (a == 0 || a < Math.Abs(c)) { a = c; s = p / 4; }
            else s = p / (2 * Math.PI) * Math.Asin(c / a);
            if (t < 1) return -.5 * (a * Math.Pow(2, 10 * (t -= 1)) * Math.Sin((t * d - s) * (2 * Math.PI) / p)) + b;
            return a * Math.Pow(2, -10 * (t -= 1)) * Math.Sin((t * d - s) * (2 * Math.PI) / p) * .5 + c + b;
        }

        /**
         * Easing equation function for an elastic (exponentially decaying sine wave) easing out/in: deceleration until halfway, then acceleration.
         *
         * @param t		Current time (in seconds).
         * @param b		Starting value.
         * @param c		Change needed in value.
         * @param d		Expected easing duration (in seconds).
         * @param a		Amplitude.
         * @param p		Period.
         * @return		The correct value.
         */
        public static double EaseOutInElastic(params double[] args)
        {
            double t; double b; double c; double d; double a; double p;
            t = args[0]; b = args[1]; c = args[2]; d = args[3]; a = ((args.Length < 5) ? 0 : args[4]); p = ((args.Length < 6) ? 0 : args[5]);

            if (t < d / 2) return EaseOutElastic(t * 2, b, c / 2, d, a, p);
            return EaseInElastic((t * 2) - d, b + c / 2, c / 2, d, a, p);
        }

        /**
         * Easing equation function for a back (overshooting cubic easing: (s+1)*t^3 - s*t^2) easing in: accelerating from zero velocity.
         *
         * @param t		Current time (in seconds).
         * @param b		Starting value.
         * @param c		Change needed in value.
         * @param d		Expected easing duration (in seconds).
         * @param s		Overshoot amount: higher s means greater overshoot (0 produces cubic easing with no overshoot, and the default value of 1.70158 produces an overshoot of 10 percent).
         * @return		The correct value.
         */
        public static double EaseInBack(params double[] args)
        {
            double t; double b; double c; double d; double s;
            t = args[0]; b = args[1]; c = args[2]; d = args[3]; s = ((args.Length < 5) ? 0 : args[4]);

            if (s == 0) s = 1.70158;
            return c * (t /= d) * t * ((s + 1) * t - s) + b;
        }

        /**
         * Easing equation function for a back (overshooting cubic easing: (s+1)*t^3 - s*t^2) easing out: decelerating from zero velocity.
         *
         * @param t		Current time (in seconds).
         * @param b		Starting value.
         * @param c		Change needed in value.
         * @param d		Expected easing duration (in seconds).
         * @param s		Overshoot amount: higher s means greater overshoot (0 produces cubic easing with no overshoot, and the default value of 1.70158 produces an overshoot of 10 percent).
         * @return		The correct value.
         */
        public static double EaseOutBack(params double[] args)
        {
            double t; double b; double c; double d; double s;
            t = args[0]; b = args[1]; c = args[2]; d = args[3]; s = ((args.Length < 5) ? 0 : args[4]);

            if (s == 0) s = 1.70158;
            return c * ((t = t / d - 1) * t * ((s + 1) * t + s) + 1) + b;
        }

        /**
         * Easing equation function for a back (overshooting cubic easing: (s+1)*t^3 - s*t^2) easing in/out: acceleration until halfway, then deceleration.
         *
         * @param t		Current time (in seconds).
         * @param b		Starting value.
         * @param c		Change needed in value.
         * @param d		Expected easing duration (in seconds).
         * @param s		Overshoot amount: higher s means greater overshoot (0 produces cubic easing with no overshoot, and the default value of 1.70158 produces an overshoot of 10 percent).
         * @return		The correct value.
         */
        public static double EaseInOutBack(params double[] args)
        {
            double t; double b; double c; double d; double s;
            t = args[0]; b = args[1]; c = args[2]; d = args[3]; s = ((args.Length < 5) ? 0 : args[4]);

            if (s == 0) s = 1.70158;
            if ((t /= d / 2) < 1) return c / 2 * (t * t * (((s *= (1.525)) + 1) * t - s)) + b;
            return c / 2 * ((t -= 2) * t * (((s *= (1.525)) + 1) * t + s) + 2) + b;
        }

        /**
         * Easing equation function for a back (overshooting cubic easing: (s+1)*t^3 - s*t^2) easing out/in: deceleration until halfway, then acceleration.
         *
         * @param t		Current time (in seconds).
         * @param b		Starting value.
         * @param c		Change needed in value.
         * @param d		Expected easing duration (in seconds).
         * @param s		Overshoot amount: higher s means greater overshoot (0 produces cubic easing with no overshoot, and the default value of 1.70158 produces an overshoot of 10 percent).
         * @return		The correct value.
         */
        public static double EaseOutInBack(params double[] args)
        {
            double t; double b; double c; double d; double s;
            t = args[0]; b = args[1]; c = args[2]; d = args[3]; s = ((args.Length < 5) ? 0 : args[4]);

            if (t < d / 2) return EaseOutBack(t * 2, b, c / 2, d, s);
            return EaseInBack((t * 2) - d, b + c / 2, c / 2, d, s);
        }

        /**
         * Easing equation function for a bounce (exponentially decaying parabolic bounce) easing in: accelerating from zero velocity.
         *
         * @param t		Current time (in seconds).
         * @param b		Starting value.
         * @param c		Change needed in value.
         * @param d		Expected easing duration (in seconds).
         * @return		The correct value.
         */
        public static double EaseInBounce(params double[] args)
        {
            double t; double b; double c; double d;
            t = args[0]; b = args[1]; c = args[2]; d = args[3];

            return c - EaseOutBounce(d - t, 0, c, d) + b;
        }

        /**
         * Easing equation function for a bounce (exponentially decaying parabolic bounce) easing out: decelerating from zero velocity.
         *
         * @param t		Current time (in seconds).
         * @param b		Starting value.
         * @param c		Change needed in value.
         * @param d		Expected easing duration (in seconds).
         * @return		The correct value.
         */
        public static double EaseOutBounce(params double[] args)
        {
            double t; double b; double c; double d;
            t = args[0]; b = args[1]; c = args[2]; d = args[3];

            if ((t /= d) < (1 / 2.75))
            {
                return c * (7.5625 * t * t) + b;
            }
            else if (t < (2 / 2.75))
            {
                return c * (7.5625 * (t -= (1.5 / 2.75)) * t + .75) + b;
            }
            else if (t < (2.5 / 2.75))
            {
                return c * (7.5625 * (t -= (2.25 / 2.75)) * t + .9375) + b;
            }
            else
            {
                return c * (7.5625 * (t -= (2.625 / 2.75)) * t + .984375) + b;
            }
        }

        /**
         * Easing equation function for a bounce (exponentially decaying parabolic bounce) easing in/out: acceleration until halfway, then deceleration.
         *
         * @param t		Current time (in seconds).
         * @param b		Starting value.
         * @param c		Change needed in value.
         * @param d		Expected easing duration (in seconds).
         * @return		The correct value.
         */
        public static double EaseInOutBounce(params double[] args)
        {
            double t; double b; double c; double d;
            t = args[0]; b = args[1]; c = args[2]; d = args[3];

            if (t < d / 2) return EaseInBounce(t * 2, 0, c, d) * .5 + b;
            else return EaseOutBounce(t * 2 - d, 0, c, d) * .5 + c * .5 + b;
        }

        /**
         * Easing equation function for a bounce (exponentially decaying parabolic bounce) easing out/in: deceleration until halfway, then acceleration.
         *
         * @param t		Current time (in seconds).
         * @param b		Starting value.
         * @param c		Change needed in value.
         * @param d		Expected easing duration (in seconds).
         * @return		The correct value.
         */
        public static double EaseOutInBounce(params double[] args)
        {
            double t; double b; double c; double d;
            t = args[0]; b = args[1]; c = args[2]; d = args[3];

            if (t < d / 2) return EaseOutBounce(t * 2, b, c / 2, d);
            return EaseInBounce((t * 2) - d, b + c / 2, c / 2, d);
        }

    }
}