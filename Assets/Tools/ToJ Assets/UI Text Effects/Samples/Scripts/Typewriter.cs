using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
[RequireComponent(typeof(LimitVisibleCharacters))]
public class Typewriter : MonoBehaviour
{
	public float delayBetweenSymbols = 0.1f;
	public AudioClip[] typeSoundEffects;
	public AudioSource audioSourceForTypeEffect;

	private float _timer = 0;
	private LimitVisibleCharacters _limitVisibleCharactersComponent = null;
	private Text _textComponent = null;

	void Start ()
	{

	}

	void Update ()
	{

	}

	private void OnEnable ()
	{
		if (_limitVisibleCharactersComponent == null)
		{
			_limitVisibleCharactersComponent = GetComponent<LimitVisibleCharacters>();
		}
		if (_textComponent == null)
		{
			_textComponent = GetComponent<Text>();
		}

		StopCoroutine("PlayTypewriter");
		StartCoroutine("PlayTypewriter");
	}

	private void OnDisable ()
	{
		StopCoroutine("PlayTypewriter");
	}

	private IEnumerator PlayTypewriter()
	{
		_timer = 0;
		_limitVisibleCharactersComponent.visibleCharacterCount = 0;
		yield return null;
		while (_limitVisibleCharactersComponent.visibleCharacterCount <= _textComponent.text.Length)
		{
			_timer += Time.deltaTime;
			if ((typeSoundEffects != null) && (typeSoundEffects.Length > 0) && (audioSourceForTypeEffect != null) && (_limitVisibleCharactersComponent.visibleCharacterCount != (int)(_timer / delayBetweenSymbols)))
			{
				audioSourceForTypeEffect.PlayOneShot(typeSoundEffects[UnityEngine.Random.Range(0, typeSoundEffects.Length)]);
			}
			_limitVisibleCharactersComponent.visibleCharacterCount = (int)(_timer / delayBetweenSymbols);
			yield return null;
		}
		_limitVisibleCharactersComponent.visibleCharacterCount = _textComponent.text.Length;
	}
}
