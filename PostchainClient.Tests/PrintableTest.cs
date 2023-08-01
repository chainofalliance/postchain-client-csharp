using Microsoft.VisualStudio.TestPlatform.Utilities;
using System;
using System.IO;
using Xunit.Abstractions;

namespace Chromia.Tests
{
    public class PrintableTest
    {
        public PrintableTest(ITestOutputHelper output)
        {
            Console.SetOut(new TestWriter(output));
        }
    }

    public class TestWriter : TextWriter
    {
        private readonly ITestOutputHelper _output;

        public TestWriter(ITestOutputHelper output)
        {
            _output = output;
            Console.SetOut(this);
        }

        public override System.Text.Encoding Encoding { get; } // set some if required

        public override void WriteLine(string value)
        {
            value ??= "<null>";
            _output.WriteLine(value);
        }
    }
}