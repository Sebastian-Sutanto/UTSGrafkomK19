using LearnOpenTK.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    internal class Asset2d
    {
        float[] _vertices =
        {

        };

        uint[] _indices =
         {

         };

        int vertexBufferObject;
        int elementBufferObject;
        int vertexArrayObject;
        Shader shader;
        int index;
        int[] _pascal;

        public Asset2d(float[] vertices, uint[] indices)
        {
            _vertices = vertices;
            _indices = indices;
            index = 0;
        }

        public void load(string shadervert, string shaderfrag)
        {
            //Initialization
            vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(vertexArrayObject);
            //parameter 1 --> variable vertices nya itu disimpan di shader index ke berapa?
            //parameter 2 --> didalam variable vertices, ada berapa vertex?
            //parameter 3  --> jenis vertex yang dikirim typenya apa?
            //parameter 4 --> datanya perlu dinormalisasi ndak?
            //parameter 5 --> dalam 1 vertex atau 1 baris itu mengandung berapa banyak titik?
            //parameter 6 --> data yang mau diolah mulai dari vertex ke berapa?
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            /*GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);*/

            if (_indices.Length != 0)
            {
                elementBufferObject = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);
                GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);
            }

            shader = new Shader(shadervert, shaderfrag);
            shader.Use();
        }

        public void render(Vector4 color, int pilihan)
        {
            shader.Use();

            int vertexColorLocation = GL.GetUniformLocation(shader.Handle, "ourColor");

            GL.Uniform4(vertexColorLocation, color);

            GL.BindVertexArray(vertexArrayObject);

            if (_indices.Length != 0)
            {
                GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
            }
            else
            {
                if (pilihan == 0)
                {
                    GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
                }
                else if (pilihan == 1)
                {
                    GL.DrawArrays(PrimitiveType.TriangleFan, 0, (_vertices.Length + 1) / 3);
                }
                else if (pilihan == 2)
                {
                    GL.DrawArrays(PrimitiveType.LineStrip, 0, index);
                }
                else if (pilihan == 3)
                {
                    GL.DrawArrays(PrimitiveType.LineStrip, 0, (_vertices.Length + 1) / 3);
                }
            }
        }

        public void createCirlce(float center_x, float center_y, float radius)
        {
            _vertices = new float[1080];
            for (int i = 0; i < 360; i++)
            {
                double degInRad = i * Math.PI / 180;

                //x
                _vertices[i * 3] = radius * (float)Math.Cos(degInRad) + center_x;

                //y
                _vertices[i * 3 + 1] = radius * (float)Math.Sin(degInRad) + center_y;

                //z
                _vertices[i * 3 + 2] = 0;
            }
        }
        public void createElips(float center_x, float center_y, float radiusX, float radiusY)
        {
            _vertices = new float[1080];
            for (int i = 0; i < 360; i++)
            {
                double degInRad = i * Math.PI / 180;

                //x
                _vertices[i * 3] = radiusX * (float)Math.Cos(degInRad) + center_x;

                //y
                _vertices[i * 3 + 1] = radiusY * (float)Math.Sin(degInRad) + center_y;

                //z
                _vertices[i * 3 + 2] = 0;
            }
        }

        public void updateMousePosition(float _x, float _y)
        {
            _vertices[index * 3] = _x;
            _vertices[index * 3 + 1] = _y;
            _vertices[index * 3 + 2] = 0;
            index++;

            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        }

        public List<int> getRow(int rowIndex)
        {
            List<int> currow = new List<int>();
            //------
            currow.Add(1);

            if (rowIndex == 0)
            {
                return currow;
            }

            //-----
            List<int> prev = getRow(rowIndex - 1);

            for (int i = 1; i < prev.Count; i++)
            {
                int curr = prev[i - 1] + prev[i];
                currow.Add(curr);
            }

            currow.Add(1);
            return currow;
        }

        public List<float> createCurveBezier()
        {
            List<float> _vertices_bezier = new List<float>();
            List<int> pascal = getRow(index - 1);
            _pascal = pascal.ToArray();

            for (float t = 0; t <= 1.0f; t += 0.01f)
            {
                Vector2 p = getP(index, t);
                _vertices_bezier.Add(p.X);
                _vertices_bezier.Add(p.Y);
                _vertices_bezier.Add(0);
            }

            return _vertices_bezier;
        }

        public Vector2 getP(int n, float t)
        {
            Vector2 p = new Vector2(0, 0);
            float[] k = new float[n];

            for (int i = 0; i < n; i++)
            {
                k[i] = (float)Math.Pow((1 - t), n - 1 - i) * (float)Math.Pow(t, i) * _pascal[i];
                p.X += k[i] * _vertices[i * 3];
                p.Y += k[i] * _vertices[i * 3 + 1];
            }

            return p;
        }

        public bool getVerticesLength()
        {
            if (_vertices[0] == 0)
            {
                return false;
            }

            if ((_vertices.Length + 1) / 3 > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void setVertices(float[] _temp)
        {
            _vertices = _temp;
        }
    }
}
