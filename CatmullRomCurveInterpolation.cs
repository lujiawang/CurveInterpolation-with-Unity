using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CatmullRomCurveInterpolation : MonoBehaviour {
	
	const int NumberOfPoints = 8;
	Vector3[] controlPoints;

    public float tau = 0.5f;
	
	const int MinX = -5;
	const int MinY = -5;
	const int MinZ = 0;

	const int MaxX = 5;
	const int MaxY = 5;
	const int MaxZ = 5;
	
	double time = 0;
	const double DT = 0.005;

    int count = 0;

    LineRenderer lr;

    double[] superSample;
    double arcLength = 0;
    double[] length;
    

    public Text text;
    bool bezier = false;


	/* Returns a point on a cubic Catmull-Rom/Blended Parabolas curve
	 * u is a scalar value from 0 to 1
	 * segment_number indicates which 4 points to use for interpolation
	 */
	Vector3 ComputePointOnCatmullRomCurve(double u, int segmentNumber)
	{
		Vector3 point = new Vector3();

        //four points
        Vector3 pi_2 = controlPoints[(NumberOfPoints + segmentNumber - 2) % NumberOfPoints];
        Vector3 pi_1 = controlPoints[(NumberOfPoints + segmentNumber - 1) % NumberOfPoints];
        Vector3 pi = controlPoints[segmentNumber];
        Vector3 pi_0 = controlPoints[(segmentNumber + 1) % NumberOfPoints]; //pi+1

        // compute and return a point as a Vector3		
        // Hint: Points on segment number 0 start at controlPoints[0] and end at controlPoints[1]
        //		 Points on segment number 1 start at controlPoints[1] and end at controlPoints[2]
        //		 etc...

        Vector3 c3 = -tau * pi_2 + (2-tau) * pi_1 + (tau-2) * pi + tau * pi_0;
        Vector3 c2 = 2 * tau * pi_2 + (tau - 3) * pi_1 + (3 - 2 * tau) * pi - tau * pi_0;
        Vector3 c1 = -tau * pi_2 + tau * pi;
        Vector3 c0 = pi_1;

        float uf = (float)u;
        point = uf * uf * uf * c3 + uf * uf * c2 + uf * c1 + c0;

        return point;
	}


    Vector3 BezierInterpolation(double t, int segmentNumber)
    {
        //four points
        Vector3 pi_2 = controlPoints[(NumberOfPoints + segmentNumber - 2) % NumberOfPoints];
        Vector3 pi_1 = controlPoints[(NumberOfPoints + segmentNumber - 1) % NumberOfPoints];
        Vector3 pi = controlPoints[segmentNumber];
        Vector3 pi_0 = controlPoints[(segmentNumber + 1) % NumberOfPoints]; //pi+1

        Vector3 q = (float)(1 - t) * pi_2 + (float)t * pi_1;
        Vector3 r = (float)(1 - t) * pi_1 + (float)t * pi;
        Vector3 s = (float)(1 - t) * pi + (float)t * pi_0;

        Vector3 P = (float)(1 - t) * q + (float)t * r;
        Vector3 T = (float)(1 - t) * r + (float)t * s;

        Vector3 point = (float)(1 - t) * P + (float)t * T;

        return point;
    }


    void GenerateControlPointGeometry()
	{
		for(int i = 0; i < NumberOfPoints; i++)
		{
			GameObject tempcube = GameObject.CreatePrimitive(PrimitiveType.Cube);
			tempcube.transform.localScale -= new Vector3(0.8f,0.8f,0.8f);
			tempcube.transform.position = controlPoints[i];
		}
	}
    
    

    double getLength(int count)
    {
        double length = 0;
        Vector3 previous = ComputePointOnCatmullRomCurve(0,count);
        for (double i=0; i <= 1; i = i+0.05)
        {
            Vector3 pos = ComputePointOnCatmullRomCurve(i,count);
            length += Vector3.Distance(pos, previous);
            previous = pos;

            lr.SetPosition((int)((count + i)*20), pos);
        }
        return length;
    }
    
    // Use this for initialization
    void Start () {

		controlPoints = new Vector3[NumberOfPoints];

        superSample = new double[]
        {
            0,  0.025, 0.075,  0.1, 0.15, //0
            0.15, 0.195,   0.225,  0.35, 0.45, //1
            0.45, 0.5,  0.575, 0.65, 0.75, //2
            0.75, 0.795,  0.825, 0.9, 1, //3
            1, 0.9, 0.825, 0.795, 0.75, //4
            0.75, 0.65,  0.575, 0.5, 0.45, //5
            0.45,  0.35, 0.225, 0.195, 0.15, //6
            0.15, 0.1, 0.075, 0.025, 0 //7
        }; //u=0.05*index;

		
		// set points randomly...
		controlPoints[0] = new Vector3(0,0,0);
		for(int i = 1; i < NumberOfPoints; i++)
		{
			controlPoints[i] = new Vector3(Random.Range(MinX,MaxX),Random.Range(MinY,MaxY),Random.Range(MinZ,MaxZ));
		}


        /*...or hard code them for testing
		controlPoints[0] = new Vector3(-1,1,0);
		controlPoints[1] = new Vector3(1,1,0);
		controlPoints[2] = new Vector3(1,-2,0);
		controlPoints[3] = new Vector3(-2,-2,0);
		controlPoints[4] = new Vector3(-2,3,0);
		controlPoints[5] = new Vector3(3,3,0);
		controlPoints[6] = new Vector3(3,-3,0);
		controlPoints[7] = new Vector3(-8,-3,0);
        */

        GenerateControlPointGeometry();

        lr = GetComponent<LineRenderer>();
        lr.positionCount = 20 * 8;
        lr.SetPosition(0,controlPoints[0]);

        length = new double[NumberOfPoints];
        length[0] = getLength(0);
        arcLength += length[0];
        for(int i=1; i<NumberOfPoints; i++)
        {
            length[i] = getLength(i);
            arcLength += length[i];
        }
        
    }

	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.B))
        {
            bezier = !bezier;
            time = 0;
        }

        if (bezier)
        {
            lr.enabled = false;
            text.text = "Bezier Interpolation";
            time += DT;
            Vector3 pos = BezierInterpolation(time, count);
            transform.position = pos;

            if (Vector3.Distance(pos, controlPoints[(count + 1) % NumberOfPoints]) <= 0.05)
            {
                count = (count + 3) % NumberOfPoints;
                time = 0;
                
            }
        }
        else
        {
            lr.enabled = true;
            text.text = "Catmull-Rom Interpolation";
            time += DT;

            //double s = -2  * Mathf.Pow((float)time, 3) + 3 * time * time;
            double s = Mathf.Abs(1 - (Mathf.Sin((float)time * Mathf.PI - Mathf.PI / 2) + 1) / 2);
            
            // use time to determine values for u and segment_number in this function call
            double u = 0;
            int index = 0;

            for (int i = 0; i < superSample.Length; i++)
            {
                if (superSample[i] >= s)
                {
                    index = i;
                    if (time % 2 <= 1)
                        break;
                }
            }
            count = index / 5;
            u = index % 5 * 0.25;

            Vector3 pos = ComputePointOnCatmullRomCurve(u, count);
            transform.position = pos;
            
        }
	}
}
