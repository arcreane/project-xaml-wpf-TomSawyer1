namespace MauiApp_Dessiner;

public partial class MainPage : ContentPage
{
	int count = 0;

	public MainPage()
	{
		InitializeComponent();
	}

	private void OnCounterClicked(object sender, EventArgs e)
	{
		count++;

		if (count == 1)
			CounterBtn.Text = $"Clicked {count} time";
		else
			CounterBtn.Text = $"Clicked {count} times";

		SemanticScreenReader.Announce(CounterBtn.Text);
	}
	//new fonction de test
	private void thomasclique(object sender, EventArgs e)
	{
		// si cliqué 
		btnthomas.Text = $"le bouton thomas est cliqué ";
		SemanticScreenReader.Announce(btnthomas.Text);
	}
}

