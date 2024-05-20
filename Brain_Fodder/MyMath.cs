using Brain_Fodder;
using OpenTK.Mathematics;

namespace SpaceEngine.Util
{
    internal class MyMath
    {
        public static Random rand = new Random();

        public static Vector3 rng3D()
        {
            return new Vector3(rand.NextSingle(), rand.NextSingle(), rand.NextSingle());
        }
        public static Vector3 rng3DMinusPlus()
        {
            return (new Vector3(rngMinusPlus(), rngMinusPlus(), rngMinusPlus()));
        }
        public static Vector2 rng2D()
        {
            return new Vector2(rand.NextSingle(), rand.NextSingle());
        }
        public static Vector2 rng2DMinusPlus()
        {
            return (new Vector2(rngMinusPlus(), rngMinusPlus()));
        }
        public static float rngMinusPlus()
        {
            return (rand.NextSingle() * 2f - 1f);
        }
        public static float rng()
        {
            return rand.NextSingle();
        }
  
        public static Vector3 reflect(Vector3 vector, Vector3 normal)
        {
            return vector-(2f * Vector3.Dot(normal, vector)*normal);
        }

        public static Matrix4 createTransformationMatrix(Transformation2D transformation)
        
        {
            return createTransformationMatrix(transformation.position, transformation.rotation, transformation.scale);
        }
        public static Matrix4 createTransformationMatrix(Vector3 position)
        {

            Matrix4 matrix = Matrix4.Identity;
            matrix = matrix * Matrix4.CreateTranslation(position);
            return matrix;
        }

        public static Matrix4 createTransformationMatrix(Vector2 position, float rotation, Vector2 scale)
        {

            Matrix4 matrix = Matrix4.Identity;
            matrix = matrix * Matrix4.CreateScale(scale.X, scale.Y, 0);
            matrix = matrix * Matrix4.CreateRotationZ(rotation);
            matrix = matrix * Matrix4.CreateTranslation(position.X, position.Y, 0);
            return matrix;
        }

        public static Matrix4 createViewMatrix(Vector3 position, Vector3 rotation)
        {

            Matrix4 matrix = Matrix4.Identity;
            matrix = matrix * Matrix4.CreateTranslation(-position);
            matrix = matrix * createRotationMatrix(rotation);
            return matrix;
        }
        public static Matrix4 createRotationMatrix(Vector3 rotation)
        {

            Matrix4 matrix = Matrix4.Identity;
            matrix = matrix * Matrix4.CreateRotationZ(rotation.Z);
            matrix = matrix * Matrix4.CreateRotationY(rotation.Y);
            matrix = matrix * Matrix4.CreateRotationX(rotation.X);
            return matrix;
        }
        public static float clamp(float number, float min, float max)
        {
            if (number < min) return min;
            if (number > max) return max;
            return number;
        }
        public static float clamp01(float number)
        {
            return clamp(number, 0.0f, 1.0f);
        }
        public static float lerp(float amount, float left, float right)
        {
            return (1.0f - amount) * left + amount * right;
        }
        public static Vector3 lerp(float amount, Vector3 left, Vector3 right)
        {
            return (1.0f - amount) * left + amount * right;
        }
        public static Vector3 calculateFaceNormal(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            float aX, aY, aZ, bX, bY, bZ;

            aX = v2.X - v1.X;
            aY = v2.Y - v1.Y;
            aZ = v2.Z - v1.Z;

            bX = v3.X - v1.X;
            bY = v3.Y - v1.Y;
            bZ = v3.Z - v1.Z;

            Vector3 normal = new Vector3((aY * bZ) - (aZ * bY), (aZ * bX) - (aX * bZ), (aX * bY) - (aY * bX));
            normal.Normalize();
            return normal;
        }
        public static float barryCentric(Vector3 p1, Vector3 p2, Vector3 p3, Vector2 pos)
        {
            float det = (p2.Z - p3.Z) * (p1.X - p3.X) + (p3.X - p2.X) * (p1.Z - p3.Z);
            float l1 = ((p2.Z - p3.Z) * (pos.X - p3.X) + (p3.X - p2.X) * (pos.Y - p3.Z)) / det;
            float l2 = ((p3.Z - p1.Z) * (pos.X - p3.X) + (p1.X - p3.X) * (pos.Y - p3.Z)) / det;
            float l3 = 1.0f - l1 - l2;
            return l1 * p1.Y + l2 * p2.Y + l3 * p3.Y;
        }
    }
}
