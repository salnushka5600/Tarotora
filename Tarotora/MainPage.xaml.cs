﻿using Tarotora.BD;

namespace Tarotora
{
    public partial class MainPage : ContentPage
    {
       public DBfuncional db = new DBfuncional();
        public MainPage()
        {
            InitializeComponent();
            db.SeedCards();
        }

        private async void Addkard(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Addcards(db));
        }

        private async void Prosmotrkolod(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Prosmotrkolod(db));
        }

        private async void Check(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Check(db));
        }
    }

}
