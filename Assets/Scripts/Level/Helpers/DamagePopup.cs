using UnityEngine;

namespace MainGame
{
    public class DamagePopup : MonoBehaviour
    {
        [SerializeField] private DamageText _damageText;

        private Vector3 _randomizeIntensity = new Vector3(0.2f, 0.17f, 0);
        private string _damageColorString = "E7B21A";
        private string _whiteColorString = "E0E0E0";

        void Start()
        {
            transform.localPosition += new Vector3(Random.Range(-_randomizeIntensity.x, _randomizeIntensity.x),
                                            Random.Range(-_randomizeIntensity.y, _randomizeIntensity.y),
                                            Random.Range(-_randomizeIntensity.z, _randomizeIntensity.z));
        }

        public void SetUp(float dmg, bool displayOnSelf = false)
        {
            var color = displayOnSelf ? UtilsClass.GetColorFromString(_damageColorString)
                                      : UtilsClass.GetColorFromString(_whiteColorString);
            _damageText.Display(dmg, color);
            //if (displayOnSelf)
            //    transform.GetChild(0).GetComponent<TextMeshPro>().color = GetColorFromString("FF0300");
            //transform.GetChild(0).GetComponent<TextMeshPro>().SetText(Mathf.RoundToInt(dmg).ToString());
        }
    }
}
