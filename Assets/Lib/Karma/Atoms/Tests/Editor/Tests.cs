

using System;
using NUnit.Framework;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Atoms.Tests
{

    class Tests
    {
        [Test]
        public void Test1() {
            var n = 0;

            Atom.While(() => n < 20, () =>
            {
                n++;
            })
            .Then(() =>
            {
                throw new Exception("ERRRORR");
            })
            .CatchError<Exception>((e) =>
            {
                //Debug.Log(e);
            })
            .Then(() =>
            {
                //Debug.Log("Todo esta bien");
            })
            .Run();

            
        }

        public IEnumerable Algo() {
            var n = 0;

            while (n < 20) {
                n++;
                yield return null;
            }
        }


       
    }
}
