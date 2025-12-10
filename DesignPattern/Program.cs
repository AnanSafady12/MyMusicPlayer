using DesignPattern.compositeDrawing;
using System;
using System.Collections.Generic;
using static System.Console;

namespace DesignPattern
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Client client = new Client();

            //// This way the client code can support the simple leaf
            //// components...
            //Leaf leaf = new Leaf();
            //Console.WriteLine("Client: I get a simple component:");
            //client.ClientCode(leaf);

            //// ...as well as the complex composites.
            //Composite tree = new Composite();
            //Composite branch1 = new Composite();
            //branch1.Add(new Leaf());
            //branch1.Add(new Leaf());
            //Composite branch2 = new Composite();
            //branch2.Add(new Leaf());
            //tree.Add(branch1);
            //tree.Add(branch2);
            //Console.WriteLine("Client: Now I've got a composite tree:");
            //client.ClientCode(tree);

            //Console.Write("Client: I don't need to check the components classes even when managing the tree:\n");
            //client.ClientCode2(tree, leaf);


            // Create a tree structure 
            CompositeElement root = new CompositeElement("Picture");
            root.Add(new PrimitiveElement("Red Line"));
            root.Add(new PrimitiveElement("Blue Circle"));
            root.Add(new PrimitiveElement("Green Box"));
            // Create a branch
            CompositeElement comp = new CompositeElement("Two Circles");
            comp.Add(new PrimitiveElement("Black Circle"));
            comp.Add(new PrimitiveElement("White Circle"));
            root.Add(comp);
            // Add and remove a PrimitiveElement
            PrimitiveElement pe = new PrimitiveElement("Yellow Line");
            root.Add(pe);
            root.Remove(pe);
            // Recursively display nodes
            root.Display(1);
            // Wait for user
            Console.ReadKey();
        }
    }
}
