namespace Md5Properties
{
    using System;

    public class Foo
    {
        public int Id { get; set; }

        [Signed]
        public DateTime Timestamp { get; set; }

        [Signed]
        public string Name { get; set; }

        [Signed]
        public decimal Price { get; set; }

        [Signed]
        public SomeEnum Thing { get; set; }
    }
}
