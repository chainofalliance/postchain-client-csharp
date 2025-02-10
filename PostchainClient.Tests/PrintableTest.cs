using System;
using System.IO;
using Xunit.Abstractions;

namespace Chromia.Tests
{
    public class PrintableTest : IDisposable
    {
        private readonly TextWriter originalOut;

        public PrintableTest(ITestOutputHelper output)
        {
            originalOut = Console.Out;
            Console.SetOut(new TestWriter(output));
        }

        public void Dispose()
        {
            Console.SetOut(originalOut);
        }
    }

    public class TestWriter : TextWriter
    {
        private readonly ITestOutputHelper _output;

        public TestWriter(ITestOutputHelper output)
        {
            _output = output;
        }

        public override System.Text.Encoding Encoding => System.Text.Encoding.UTF8;

        public override void WriteLine(string value)
        {
            value ??= "<null>";
            _output.WriteLine(value);
        }

        public override void WriteLine(object value)
        {
            WriteLine(value?.ToString());
        }

        public override void Write(string value)
        {
            _output.WriteLine(value);
        }

        public override void Write(object value)
        {
            Write(value?.ToString());
        }
    }
}