using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignPattern.compositeDrawing
{
    public abstract class DrawingElement
    {
        protected string name;
        // Constructor
        public DrawingElement(string name)
        {
            this.name = name;
        }
        public abstract void Add(DrawingElement d);
        public abstract void Remove(DrawingElement d);
        public abstract void Display(int indent);
    }
    /// <summary>
    /// The 'Leaf' class
    /// </summary>
    public class PrimitiveElement : DrawingElement
    {
        // Constructor
        public PrimitiveElement(string name)
            : base(name)
        {
        }
        public override void Add(DrawingElement c)
        {
            Console.WriteLine(
                "Cannot add to a PrimitiveElement");
        }
        public override void Remove(DrawingElement c)
        {
            Console.WriteLine(
                "Cannot remove from a PrimitiveElement");
        }
        public override void Display(int indent)
        {
            Console.WriteLine(
                new String('-', indent) + " " + name);
        }
    }

    /// <summary>
    /// The 'Composite' class
    /// </summary>
    public class CompositeElement : DrawingElement
    {
        List<DrawingElement> elements = new List<DrawingElement>();
        // Constructor
        public CompositeElement(string name)
            : base(name)
        {
        }
        public override void Add(DrawingElement d)
        {
            elements.Add(d);
        }
        public override void Remove(DrawingElement d)
        {
            elements.Remove(d);
        }
        public override void Display(int indent)
        {
            Console.WriteLine(new String('-', indent) +
                "+ " + name);
            // Display each child element on this node
            foreach (DrawingElement d in elements)
            {
                d.Display(indent + 2);
            }
        }
    }
}
