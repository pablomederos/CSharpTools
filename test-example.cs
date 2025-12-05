namespace ExampleNamespace
{
    public class ExampleClass
    {
        private static readonly int MaxValue = 100;
        
        public string Name { get; set; }
        
        public static void StaticMethod()
        {
            var localVariable = 42;
            Console.WriteLine(localVariable);
        }
        
        public abstract class AbstractBase
        {
            public abstract void AbstractMethod();
        }
    }
    
    public interface IExampleInterface
    {
        void InterfaceMethod(string parameter);
    }
    
    public enum ExampleEnum
    {
        First,
        Second,
        Third
    }
    
    public struct ExampleStruct
    {
        public int X;
        public int Y;
    }
}
