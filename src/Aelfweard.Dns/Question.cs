namespace Aelfweard.Dns
{
    public class Question
    {
        public string Name { get; }
        public Type Type { get; }
        public Class Class { get; }

        public Question(string name, Type type, Class @class) {
            Name = name;
            Type = type;
            Class = @class;
        }
    }
}
