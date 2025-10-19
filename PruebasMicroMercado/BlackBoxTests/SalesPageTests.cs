using OpenQA.Selenium;
using System.Collections.Generic;
using Xunit;

namespace PruebasMicroMercado.BlackBoxTests
{
    [Collection("SeleniumTests")]
    public class SalesPageTests
    {
        private readonly WebDriverFixture _fixture;
        private readonly PageHelpers _page;

        public SalesPageTests(WebDriverFixture fixture)
        {
            _fixture = fixture;
            _page = new PageHelpers(_fixture.Driver);
        }

        [Fact(DisplayName = "Full Sales Test - All Products (Fixed Qty=1)")]
        public void FullSalesTest_AllProducts_FixedQuantity()
        {
            _page.GoTo("https://localhost:7040/Sales");
            _page.ClearCart();

            string clientTax = "12345678";
            _page.SetClientTaxDocument(clientTax);
            _page.SetPaymentType(1); // Cash

            var products = new List<(string Name, decimal Price)>
            {
                ("Yogurt Bebible Sabor Durazno Pil 1000 Gr", 10),
                ("Leche de Soya sabor Banana Soy 946 Ml", 8),
                ("Mantequilla con Sal Pil 900 Gr", 25),
                ("Mantequilla sin Sal Pil 200 Gr", 18),
                ("FrutaManzana Roja Abasto 500 Gr", 12),
                ("Mandarina Morocochi Abasto 500 Gr", 10),
                ("Rucula Lannin Impex", 15),
                ("Naranja Criolla Abasto 500 Gr", 20),
                ("Manzana Verde Abasto 500 Gr", 22),
                ("Pasta Dental Limpieza  180 Gr", 18),
                ("Hilo Dental con Fluor & Menta Colgate 25 M", 25),
                ("Pasta Dental Prot Anti Caries Pepsodent 90 Gr", 12),
                ("Cepillo Flex Foramen Unidad", 10),
                ("Antitranspirante en Barra Speed Stick 50 Gr", 8),
                ("Desodorante Roll on Antibacterial Rexona men 50 Ml", 15),
                ("Desodorantes y Antitranspirantes", 20),
                ("Desodorante Roll on Antibacterial Rexona men 50 Ml", 35),
                ("Prestobarba Gillette 3 Cool 4 Unds", 25),
                ("Presto Barba Confort 3 Normal Bic 2 Unds", 22),
                ("Bloqueador Solar Sport Factor 50 Solaris 90 Gr", 28),
                ("Crema Depiladora para Piel Sensible Veet 100 Ml", 30),
                ("Jabón Intimo Herbal Nosotras 200 Ml.", 25),
                ("Toalla Normal Maternidad Nosotras 10 Unds", 22),
                ("Protectores Diarios", 15),
                ("Jabon intimo Nosotras", 18),
                ("Toalla Normal Maternidad Nosotras 10 Unds", 40),
                ("Preservativo Max Men Inspiral Tornado 3 Unds", 25),
                ("Detergente en polvo Matic Omo 2 Kg", 20),
                ("Suavizante Caricias Brisa De Primavera Uno 1.800 ", 25),
                ("Quitamanchas Vanish Prelavado Gatillo Blanco 500 ", 22),
                ("Jabon Delicada Uno 210 Gr", 18),
                ("Ambientador Harmony Glade Aerosol 360 Ml", 15),
                ("Aerosol Mata Todo Tyson 360 Cm3", 18),
                ("Lavavajillas Naranja Mr Flash 1050 ml", 14),
                ("Creama Brillametal Brasso Negro 70 Gr", 20),
                ("Sacagrasa con Gatillo Salpolio 650 ml", 10),
                ("Esponja Ola 3 Unds", 18),
                ("Toalla De Cocina Hogar 1 Unidad", 22),
                ("Papel Higiénico Plus Doble Hoja Elite 24 Rollos ", 20),
                ("Limpia Bano Ultra Rapido con Gatillo Cif 500 Ml", 15),
                ("Agua Lavandina Aditiva Marina X-5 1 Lt", 12),
                ("Desinfectante Bebe Lysoform 360 Ml", 14),
                ("Limpia Vidrios con Gatillo Ola 900 Ml", 18),
                ("Crema para Calzado Betun Negro Nugget 36 Gr", 15),
                ("Cepillo Multiuso para Ropa Condor Unidad", 10),
                ("Escoba Multiuso Clorinda Unidad", 12),
                ("Guante Naranja T7 1/2 Master Unidad", 8),
                ("Recogedor de Basura con Mango Movica Unidad", 10),
                ("Palo Trapeador Movica Unidad", 12),
                ("Bolsa Rayada 60 cm x 80 cm Belen 25 Unds", 14),
                ("Arroz Familiar Caisy 1 Kg", 18),
                ("Arroz Superior", 20),
                ("Arroz Integral y Especiales", 25),
                ("1 bolsa de Frijol de 50gr", 15),
                ("1 bolsa de Frutos Secos", 30),
                ("1 bolsa de Arveja 100gr", 14),
                ("1 bolsa de Garbanzo 45 gr", 18),
                ("Endulzante con Stevia Equal 50 Unds.", 12),
                ("Edulcorante de Mesa Liquido Chuker 600 Cm3", 20),
                ("Pasta al Huevo Tagliatelle Anita 400 Gr", 18),
                ("Fideo Codo Rayado Don Vittorio 400 Gr", 18),
                ("Fideo Cabello de Angel Don Vittorio 400 Gr", 25),
                ("Fideos Ramen Sabor Costilla Nissin 85 Gr", 28),
                ("Pure de Papas Kris 250 Gr", 15),
                ("Sopa Familiar Pollos con Fideos Cab", 12),
                ("Sopa Crema Zapallo Knorr 70 Gr", 14),
                ("Sopa de Pollo Maruchan 85 Gr", 10),
                ("Cafe Frasco Clasico Iguaçu 100 Gr", 25),
                ("Hojuelas de Avena y Chia ChiAvena Princesa 300 Gr", 22),
                ("Chocolate en polvo Chocolike 1000 Gr", 18),
                ("Dulce de Leche Sancor 1 Kg", 20),
                ("Papa Original Pringles 149 Gr", 10),
                ("Pipoca sabor Mantequilla Act II 91 Gr", 12),
                ("Mix de Frutos Secos Varios Maya 260 Gr", 15),
                ("Nachos de Maiz Tradicional Mexicamba 185 Gr", 18),
                ("Aceituna Verde Sachet Cebila 100 Gr", 25),
                ("Alcaparras Hemmer 170 Gr", 20),
                ("Atun Ensalada California Real 174 Gr", 18),
                ("Sardina Salsa de Tomate San Lucas 500 Gr", 25),
                ("Mayonesa light Kris 200 Ml", 18),
                ("Ketchup Original Doypack Kris 1000 Gr", 15),
                ("Mostaza Original Kris 400 Gr", 15),
                ("Llajua Churrasquera B&R 220 Gr", 12),
                ("Chunchulines en Bandeja x kg", 85),
                ("Hamburguesa de Carne Sin Condimento en Bandeja", 45),
                ("Langostinos Precocidos Puerto Azul 1 Kg", 60),
                ("Pechuga de pollo sin piel en Bnadeja Sofia", 35),
                ("Tender de Pollo Sofia 500 Gr", 90),
                ("Alas de Pollo en Bandeja Sofia", 70)
            };

            decimal expectedTotal = 0;

            // Add each product with quantity = 1
            foreach (var product in products)
            {
                _page.AddProductToCart(product.Name, 1);
                expectedTotal += product.Price;
            }

            _page.SetCashReceived(expectedTotal + 50);
            _page.ConfirmSale();

            decimal displayedTotal = _page.GetTotal();
            Assert.Equal(expectedTotal, displayedTotal);
        }
    }
}