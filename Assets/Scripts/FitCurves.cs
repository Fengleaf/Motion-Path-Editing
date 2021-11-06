using System.Collections;
using System.Collections.Generic;
using System.Drawing;
//using System.Numerics;
using UnityEngine;

/*
An Algorithm for Automatically Fitting Digitized Curves
by Philip J. Schneider
from "Graphics Gems", Academic Press, 1990
*/
public static class FitCurves
{
    /*  Fit the Bezier curves */

    private const int MAXPOINTS = 10000;

    public static Vector3[] GetBezierFitCurve(Vector3[] d, double error)
    {
        Vector3 tHat1, tHat2;    /*  Unit tangent vectors at endpoints */

        tHat1 = ComputeLeftTangent(d, 0);
        tHat2 = ComputeRightTangent(d, d.Length - 1);
        double[] u = ChordLengthParameterize(d, 0, d.Length - 1); ;     /*  Parameter values for point  */
        return GenerateBezier(d, 0, d.Length - 1, u, tHat1, tHat2);
    }

    public static List<Vector3> FitCurve(Vector3[] d, double error)
    {
        Vector3 tHat1, tHat2;    /*  Unit tangent vectors at endpoints */

        tHat1 = ComputeLeftTangent(d, 0);
        tHat2 = ComputeRightTangent(d, d.Length - 1);
        List<Vector3> result = new List<Vector3>();
        FitCubic(d, 0, d.Length - 1, tHat1, tHat2, error, result);
        return result;
    }

    private static double CountLen(Vector3 pntA, Vector3 pntB)
    {
        return Mathf.Sqrt((pntA.x - pntB.x) * (pntA.x - pntB.x) + (pntA.z - pntB.z) * (pntA.z - pntB.z));
    }

    private static Vector3 VectMultBy(Vector3 vec, double multiplier)
    {
        Vector3 resVec = vec;
        resVec.x = (float)(resVec.x * multiplier);
        resVec.z = (float)(resVec.z * multiplier);
        return resVec;
    }

    private static void FitCubic(Vector3[] d, int first, int last, Vector3 tHat1, Vector3 tHat2, double error, List<Vector3> result)
    {
        Vector3[] bezCurve; /*Control points of fitted Bezier curve*/
        double[] u;     /*  Parameter values for point  */
        double[] uPrime;    /*  Improved parameter values */
        double maxError;    /*  Maximum fitting error    */
        int splitVector3; /*  Vector3 to split point set at  */
        int nPts;       /*  Number of points in subset  */
        double iterationError; /*Error below which you try iterating  */
        int maxIterations = 4; /*  Max times to try iterating  */
        Vector3 tHatCenter;      /* Unit tangent vector at splitVector3 */
        int i;

        iterationError = error * error;
        nPts = last - first + 1;

        /*  Use heuristic if region only has two points in it */
        if (nPts == 2)
        {
            double dist = CountLen(d[first], d[last]) / 3.0;

            bezCurve = new Vector3[4];
            bezCurve[0] = d[first];
            bezCurve[3] = d[last];
            bezCurve[1] = VectMultBy(tHat1, dist) + bezCurve[0];
            bezCurve[2] = VectMultBy(tHat2, dist) + bezCurve[3];

            result.Add(bezCurve[1]);
            result.Add(bezCurve[2]);
            result.Add(bezCurve[3]);
            return;
        }

        /*  Parameterize points, and attempt to fit curve */
        u = ChordLengthParameterize(d, first, last);
        bezCurve = GenerateBezier(d, first, last, u, tHat1, tHat2);

        /*  Find max deviation of points to fitted curve */
        maxError = ComputeMaxError(d, first, last, bezCurve, u, out splitVector3);
        if (maxError < error)
        {
            result.Add(bezCurve[1]);
            result.Add(bezCurve[2]);
            result.Add(bezCurve[3]);
            return;
        }


        /*  If error not too large, try some reparameterization  */
        /*  and iteration */
        if (maxError < iterationError)
        {
            for (i = 0; i < maxIterations; i++)
            {
                uPrime = Reparameterize(d, first, last, u, bezCurve);
                bezCurve = GenerateBezier(d, first, last, uPrime, tHat1, tHat2);
                maxError = ComputeMaxError(d, first, last,
                           bezCurve, uPrime, out splitVector3);
                if (maxError < error)
                {
                    result.Add(bezCurve[1]);
                    result.Add(bezCurve[2]);
                    result.Add(bezCurve[3]);
                    return;
                }
                u = uPrime;
            }
        }

        /* Fitting failed -- split at max error point and fit recursively */
        tHatCenter = ComputeCenterTangent(d, splitVector3);
        FitCubic(d, first, splitVector3, tHat1, tHatCenter, error, result);
        tHatCenter.x = -tHatCenter.x;
        tHatCenter.z = -tHatCenter.z;
        FitCubic(d, splitVector3, last, tHatCenter, tHat2, error, result);
    }

    static Vector3[] GenerateBezier(Vector3[] d, int first, int last, double[] uPrime, Vector3 tHat1, Vector3 tHat2)
    {
        int i;
        Vector3[,] A = new Vector3[MAXPOINTS, 2];/* Precomputed rhs for eqn    */

        int nPts;           /* Number of pts in sub-curve */
        double[,] C = new double[2, 2];            /* Matrix C     */
        double[] X = new double[2];          /* Matrix X         */
        double det_C0_C1,      /* Determinants of matrices */
                det_C0_X,
                det_X_C1;
        double alpha_l,        /* Alpha values, left and right */
                alpha_r;
        Vector3 tmp;            /* Utility variable     */
        Vector3[] bezCurve = new Vector3[4];    /* RETURN bezier curve ctl pts  */
        nPts = last - first + 1;

        /* Compute the A's  */
        for (i = 0; i < nPts; i++)
        {
            Vector3 v1, v2;
            v1 = tHat1;
            v2 = tHat2;
            v1 = VectMultBy(v1, B1(uPrime[i]));
            v1 = VectMultBy(v2, B2(uPrime[i]));
            A[i, 0] = v1;
            A[i, 1] = v2;
        }

        /* Create the C and X matrices  */
        C[0, 0] = 0.0;
        C[0, 1] = 0.0;
        C[1, 0] = 0.0;
        C[1, 1] = 0.0;
        X[0] = 0.0;
        X[1] = 0.0;

        for (i = 0; i < nPts; i++)
        {
            C[0, 0] += V2Dot(A[i, 0], A[i, 0]);
            C[0, 1] += V2Dot(A[i, 0], A[i, 1]);
            /*                  C[1][0] += V2Dot(&A[i][0], &A[i][9]);*/
            C[1, 0] = C[0, 1];
            C[1, 1] += V2Dot(A[i, 1], A[i, 1]);

            tmp = (d[first + i] - (VectMultBy(d[first], B0(uPrime[i])) + 
                (VectMultBy(d[first], B1(uPrime[i])) + (VectMultBy(d[last], B2(uPrime[i])) +
                VectMultBy(d[last], B3(uPrime[i]))))));


            X[0] += V2Dot(A[i, 0], tmp);
            X[1] += V2Dot(A[i, 1], tmp);
        }

        /* Compute the determinants of C and X  */
        det_C0_C1 = C[0, 0] * C[1, 1] - C[1, 0] * C[0, 1];
        det_C0_X = C[0, 0] * X[1] - C[1, 0] * X[0];
        det_X_C1 = X[0] * C[1, 1] - X[1] * C[0, 1];

        /* Finally, derive alpha values */
        alpha_l = (det_C0_C1 == 0) ? 0.0 : det_X_C1 / det_C0_C1;
        alpha_r = (det_C0_C1 == 0) ? 0.0 : det_C0_X / det_C0_C1;

        /* If alpha negative, use the Wu/Barsky heuristic (see text) */
        /* (if alpha is 0, you get coincident control points that lead to
         * divide by zero in any subsequent NewtonRaphsonRootFind() call. */
        double segLength = CountLen(d[first], d[last]);
        double epsilon = 1.0e-6 * segLength;
        if (alpha_l < epsilon || alpha_r < epsilon)
        {
            /* fall back on standard (probably inaccurate) formula, and subdivide further if needed. */
            double dist = segLength / 3.0;
            bezCurve[0] = d[first];
            bezCurve[3] = d[last];
            bezCurve[1] = VectMultBy(tHat1, dist) + bezCurve[0];
            bezCurve[2] = VectMultBy(tHat2, dist) + bezCurve[3];
            return (bezCurve);
        }

        /*  First and last control points of the Bezier curve are */
        /*  positioned exactly at the first and last data points */
        /*  Control points 1 and 2 are positioned an alpha distance out */
        /*  on the tangent vectors, left and right, respectively */
        bezCurve[0] = d[first];
        bezCurve[3] = d[last];
        bezCurve[1] = VectMultBy(tHat1, alpha_l) + bezCurve[0];
        bezCurve[2] = VectMultBy(tHat2, alpha_r) + bezCurve[3];
        return (bezCurve);
    }

    /*
     *  Reparameterize:
     *  Given set of points and their parameterization, try to find
     *   a better parameterization.
     *
     */
    static double[] Reparameterize(Vector3[] d, int first, int last, double[] u, Vector3[] bezCurve)
    {
        int nPts = last - first + 1;
        int i;
        double[] uPrime = new double[nPts];      /*  New parameter values    */

        for (i = first; i <= last; i++)
        {
            uPrime[i - first] = NewtonRaphsonRootFind(bezCurve, d[i], u[i - first]);
        }
        return uPrime;
    }



    /*
     *  NewtonRaphsonRootFind :
     *  Use Newton-Raphson iteration to find better root.
     */
    static double NewtonRaphsonRootFind(Vector3[] Q, Vector3 P, double u)
    {
        double numerator, denominator;
        Vector3[] Q1 = new Vector3[3], Q2 = new Vector3[2];   /*  Q' and Q''          */
        Vector3 Q_u, Q1_u, Q2_u; /*u evaluated at Q, Q', & Q''  */
        double uPrime;     /*  Improved u          */
        int i;

        /* Compute Q(u) */
        Q_u = BezierII(3, Q, u);

        /* Generate control vertices for Q' */
        for (i = 0; i <= 2; i++)
        {
            Q1[i].x = (float)((Q[i + 1].x - Q[i].x) * 3.0);
            Q1[i].z = (float)((Q[i + 1].z - Q[i].z) * 3.0);
        }

        /* Generate control vertices for Q'' */
        for (i = 0; i <= 1; i++)
        {
            Q2[i].x = (float)((Q1[i + 1].x - Q1[i].x) * 2.0);
            Q2[i].z = (float)((Q1[i + 1].z - Q1[i].z) * 2.0);
        }

        /* Compute Q'(u) and Q''(u) */
        Q1_u = BezierII(2, Q1, u);
        Q2_u = BezierII(1, Q2, u);

        /* Compute f(u)/f'(u) */
        numerator = (Q_u.x - P.x) * (Q1_u.x) + (Q_u.z - P.z) * (Q1_u.z);
        denominator = (Q1_u.x) * (Q1_u.x) + (Q1_u.z) * (Q1_u.z) +
                      (Q_u.x - P.x) * (Q2_u.x) + (Q_u.z - P.z) * (Q2_u.z);
        if (denominator == 0.0f) return u;

        /* u = u - f(u)/f'(u) */
        uPrime = u - (numerator / denominator);
        return (uPrime);
    }



    /*
     *  Bezier :
     *      Evaluate a Bezier curve at a particular parameter value
     * 
     */
    static Vector3 BezierII(int degree, Vector3[] V, double t)
    {
        int i, j;
        Vector3 Q;          /* Vector3 on curve at parameter t    */
        Vector3[] Vtemp;      /* Local copy of control points     */

        /* Copy array   */
        Vtemp = new Vector3[degree + 1];
        for (i = 0; i <= degree; i++)
        {
            Vtemp[i] = V[i];
        }

        /* Triangle computation */
        for (i = 1; i <= degree; i++)
        {
            for (j = 0; j <= degree - i; j++)
            {
                Vtemp[j].x = (float)((1.0 - t) * Vtemp[j].x + t * Vtemp[j + 1].x);
                Vtemp[j].z = (float)((1.0 - t) * Vtemp[j].z + t * Vtemp[j + 1].z);
            }
        }

        Q = Vtemp[0];
        return Q;
    }


    /*
     *  B0, B1, B2, B3 :
     *  Bezier multipliers
     */
    static double B0(double u)
    {
        double tmp = 1.0 - u;
        return (tmp * tmp * tmp);
    }


    static double B1(double u)
    {
        double tmp = 1.0 - u;
        return (3 * u * (tmp * tmp));
    }

    static double B2(double u)
    {
        double tmp = 1.0 - u;
        return (3 * u * u * tmp);
    }

    static double B3(double u)
    {
        return (u * u * u);
    }

    /*
     * ComputeLeftTangent, ComputeRightTangent, ComputeCenterTangent :
     *Approximate unit tangents at endpoints and "center" of digitized curve
     */
    static Vector3 ComputeLeftTangent(Vector3[] d, int end)
    {
        Vector3 tHat1;
        tHat1 = d[end + 1] - d[end];
        tHat1.Normalize();
        return tHat1;
    }

    static Vector3 ComputeRightTangent(Vector3[] d, int end)
    {
        Vector3 tHat2;
        tHat2 = d[end - 1] - d[end];
        tHat2.Normalize();
        return tHat2;
    }

    static Vector3 ComputeCenterTangent(Vector3[] d, int center)
    {
        Vector3 V1, V2, tHatCenter = new Vector3();

        V1 = d[center - 1] - d[center];
        V2 = d[center] - d[center + 1];
        tHatCenter.x = (float)((V1.x + V2.x) / 2.0);
        tHatCenter.z = (float)((V1.z + V2.z) / 2.0);
        tHatCenter.Normalize();
        return tHatCenter;
    }


    /*
     *  ChordLengthParameterize :
     *  Assign parameter values to digitized points 
     *  using relative distances between points.
     */
    static double[] ChordLengthParameterize(Vector3[] d, int first, int last)
    {
        int i;
        double[] u = new double[last - first + 1];           /*  Parameterization        */

        u[0] = 0.0;
        for (i = first + 1; i <= last; i++)
        {
            u[i - first] = u[i - first - 1] + CountLen(d[i - 1], d[i]);
        }

        for (i = first + 1; i <= last; i++)
        {
            u[i - first] = u[i - first] / u[last - first];
        }

        return u;
    }




    /*
     *  ComputeMaxError :
     *  Find the maximum squared distance of digitized points
     *  to fitted curve.
    */
    static double ComputeMaxError(Vector3[] d, int first, int last, Vector3[] bezCurve, double[] u, out int splitVector3)
    {
        int i;
        double maxDist;        /*  Maximum error       */
        double dist;       /*  Current error       */
        Vector3 P;          /*  Vector3 on curve      */
        Vector3 v;          /*  Vector from point to curve  */

        splitVector3 = (last - first + 1) / 2;
        maxDist = 0.0;
        for (i = first + 1; i < last; i++)
        {
            P = BezierII(3, bezCurve, u[i - first]);
            v = P - d[i];
            dist = v.x * v.x + v.z + v.z;
            if (dist >= maxDist)
            {
                maxDist = dist;
                splitVector3 = i;
            }
        }
        return maxDist;
    }

    private static double V2Dot(Vector3 a, Vector3 b)
    {
        return ((a.x * b.x) + (a.z * b.z));
    }

}
