﻿

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;


namespace cloudscribe.Core.Web.Test
{
    /// <summary>
    /// these are just tests I wrote to make sure I really understand how c# property hiding works
    /// when the derived type is cast back to its base type
    /// </summary>
    public class CodeWorksAsIntendedTests
    {

        [Fact]
        public void Can_Derived_Class_Hide_Base_Property()
        {
            var theFoo = new Foo();
            theFoo.FooName = "Foo";

            var useInner = false;
            var theHider = new FooHider(theFoo, useInner);

            Assert.True(theHider.FooName != theFoo.FooName);

            useInner = true;
            var theRevealer = new FooHider(theFoo, useInner);
            Assert.True(theRevealer.FooName == theFoo.FooName);
        }

        [Fact]
        public void Can_Derived_Class_Hide_Base_Property_When_Cast_To_Base()
        {
            var theFoo = new Foo();
            theFoo.FooName = "Foo";

            var useInner = false;
            var theHider = new FooHider(theFoo, useInner);

            Assert.True(
                ((Foo)theHider).FooName != theFoo.FooName
                );

            Assert.True("Foo" == theFoo.FooName);
            
            var theRevealer = new FooHider(theFoo, true);

            Assert.True("Foo" == theRevealer.InnerFoo.FooName);
            
            var fooFrocess = new FooProcessor(theRevealer);
            string fooResult = fooFrocess.GimmeTheName();

            // this fails same casting issue as below
            //Assert.False(string.IsNullOrEmpty(fooResult));

            //var fooCast = (Foo)theRevealer;
            //Assert.NotNull(fooCast);

            // this failure verifies that something I was going to try won't work
            // I was hoping I could sub class CookieAuthenticationOptions and resolve some properties internally by tenant
            // to work around the fact that both the middleware instance and the cookie options are singleton
            // making it hard to do multi-tenancy 
            // but apparently casting the sub class back to CookieAuthenticationOptions
            // would just break the hidden properties

            // this fails
            //Assert.False(string.IsNullOrEmpty(fooResult));

            // this fails unfortunately
            //Assert.True("Foo" == fooCast.FooName);
            // but this also fails 
            //Assert.True("NoFooForYou" == fooCast.FooName);



        }
    }


    public class Foo
    {
        public string FooName { get; set; }
    }

    public class FooProcessor
    {
        public FooProcessor(Foo theFoo)
        {
            myFoo = theFoo;
        }

        private Foo myFoo;

        public string GimmeTheName()
        {
            return myFoo.FooName;
        }

    }

    public class FooHider : Foo
    {
        public FooHider(Foo innerFoo, bool useInner)
        {
            this.innerFoo = innerFoo;
            this.useInner = useInner;
        }

        private Foo innerFoo;

        public Foo InnerFoo
        {
            get { return innerFoo; }
        }

        private bool useInner;

        private string fooName = "NoFooForYou";
        new public string FooName
        {
            get
            {
                //return innerFoo.FooName;
                if(useInner) return innerFoo.FooName;
                return fooName;
            }
            set { fooName = value; }
        }
    }
}
