using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace MetalPod.Tests.PlayMode
{
    public class IntegrationTestStubs
    {
        [Test]
        public void PlayModeSuite_Placeholder()
        {
            Assert.Pass("PlayMode integration tests will be added in a future task.");
        }

        [UnityTest]
        public IEnumerator PlayModeCoroutine_Placeholder()
        {
            yield return null;
            Assert.Pass();
        }
    }
}
