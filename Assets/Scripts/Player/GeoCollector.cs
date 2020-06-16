using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GeoCollector : MonoBehaviour
{
    [SerializeField] Animator collectEffecter;
    [SerializeField] AudioClip[] geoCollects;
    [SerializeField] int geoCount = 0;
    [SerializeField] TextMeshProUGUI geoText;
    [SerializeField] bool needToReset;

    private AudioSource audioSource;

    private int animationCollectTrigger = Animator.StringToHash("Collect");

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (needToReset)
        {
            PlayerPrefs.SetInt("Geo", 0);
            PlayerPrefs.Save();
        }
        geoCount = PlayerPrefs.GetInt("Geo");
        geoText.text = geoCount.ToString();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Geo"))
        {
            collectEffecter.SetTrigger(animationCollectTrigger);
            int index = Random.Range(0, geoCollects.Length);
            audioSource.PlayOneShot(geoCollects[index]);
            geoCount++;
            PlayerPrefs.SetInt("Geo", geoCount);
            PlayerPrefs.Save();
            geoText.SetText(geoCount.ToString());
            Destroy(collision.gameObject);
        }
    }
}
