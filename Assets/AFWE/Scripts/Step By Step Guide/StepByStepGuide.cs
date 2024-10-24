using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class StepByStepGuideInfo {
    public string jobName;
	public List<Texture2D> stepsImages = new List<Texture2D>();
}

public class PhotoCollection {
	public string[] base64Photos; // Array to hold base64 photo strings

	// Constructor to initialize the collection
	public PhotoCollection(string[] base64PhotoArray) {
		base64Photos = base64PhotoArray;
	}

	// Method to convert base64 string to Texture2D
	public Texture2D Base64ToTexture(string base64Photo) {
		byte[] imageBytes = Convert.FromBase64String(base64Photo);
		Texture2D texture = new Texture2D(2, 2); // Create a new Texture2D object
		texture.LoadImage(imageBytes); // Load the image bytes into the texture
		return texture;
	}

	// Method to get all the textures from the base64 strings
	public Texture2D[] GetAllTextures() {
		Texture2D[] textures = new Texture2D[base64Photos.Length];
		for (int i = 0; i < base64Photos.Length; i++) {
			textures[i] = Base64ToTexture(base64Photos[i]);
		}
		return textures;
	}
}

public class PhotoConverter {
	// Method to convert a Texture2D to a base64 string
	public string TextureToBase64(Texture2D texture, bool isPNG = true) {
		byte[] imageBytes;

		// Choose the format: PNG or JPG
		if (isPNG) {
			imageBytes = texture.EncodeToPNG(); // Encode texture to PNG
		} else {
			imageBytes = texture.EncodeToJPG(); // Encode texture to JPG
		}

		// Convert byte array to base64 string
		return Convert.ToBase64String(imageBytes);
	}
}

public class StepByStepGuide : ManagerBaseScript
{
	[SerializeField] StepByStepGuideInfo guideInfo;
	[SerializeField] MRButtonClass nextButton, prevButton;
	[SerializeField] RawImage imagePanel;

	private int currentImageIndex = 0;

	protected override void Awake() {
		base.Awake();
		if (guideInfo.stepsImages.Count > 0) {
			DisplayImage(currentImageIndex);
		}

		nextButton.button.OnClicked.AddListener(delegate {
			SwitchImage(1);
		});
		prevButton.button.OnClicked.AddListener(delegate {
			SwitchImage(-1);
		});
	}

	private void SwitchImage(int direction) {
		if (guideInfo.stepsImages.Count == 0) return;
		currentImageIndex = (currentImageIndex + direction) % guideInfo.stepsImages.Count;

		if (currentImageIndex < 0) {
			currentImageIndex = guideInfo.stepsImages.Count - 1;
		}

		DisplayImage(currentImageIndex);
	}

	private void DisplayImage(int index) {
		if (guideInfo.stepsImages.Count > 0) {
			imagePanel.texture = guideInfo.stepsImages[index];
		}
	}
}
