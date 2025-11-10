using FluentValidation;
using FluentValidation.Results;
using MicroMercado.Application.DTOs.Product;
using MicroMercado.Application.Services;
using MicroMercado.Domain.Models;
using MicroMercado.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PruebasMicroMercado.WhiteBoxTests
{
    public class ProductServiceTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        private Mock<ILogger<ProductService>> GetMockLogger()
        {
            return new Mock<ILogger<ProductService>>();
        }

        private (Mock<IValidator<CreateProductDTO>> createValidator, Mock<IValidator<UpdateProductDTO>> updateValidator) GetMockValidators()
        {
            var createMock = new Mock<IValidator<CreateProductDTO>>();
            var updateMock = new Mock<IValidator<UpdateProductDTO>>();
            createMock
                .Setup(v => v.ValidateAsync(It.IsAny<CreateProductDTO>(), default))
                .ReturnsAsync(new ValidationResult());

            updateMock
                .Setup(v => v.ValidateAsync(It.IsAny<UpdateProductDTO>(), default))
                .ReturnsAsync(new ValidationResult());

            return (createMock, updateMock);
        }

        private async Task SeedTestData(ApplicationDbContext context)
        {
            var category1_limpieza = new Category { Id = 1, Name = "Productos de Limpieza", Status = (byte)1, LastUpdate = DateTime.Now };
            var category2_alimentos = new Category { Id = 2, Name = "Alimentos Diversos", Status = (byte)1, LastUpdate = DateTime.Now };
            var category3_frutasVerduras = new Category { Id = 3, Name = "Frutas y Verduras", Status = (byte)1, LastUpdate = DateTime.Now };
            var category4_carnes = new Category { Id = 4, Name = "Carnes", Status = (byte)1, LastUpdate = DateTime.Now };
            var category5_cuidadoPersonal = new Category { Id = 5, Name = "Cuidado personal", Status = (byte)1, LastUpdate = DateTime.Now };
            var category6_lacteos = new Category { Id = 6, Name = "Lacteos", Status = (byte)1, LastUpdate = DateTime.Now };
            var category7_inactiva = new Category { Id = 7, Name = "Hogar (Inactiva)", Status = (byte)0, LastUpdate = DateTime.Now };

            context.Categories.AddRange(
                category1_limpieza,
                category2_alimentos,
                category3_frutasVerduras,
                category4_carnes,
                category5_cuidadoPersonal,
                category6_lacteos,
                category7_inactiva
            );
            var products = new[]
            {
                new Product { Id = 1, Name = "Yogurt Bebible Sabor Durazno", Description = "Lacteos", Brand = "Pil", Price = 10m, Stock = 50, CategoryId = 6, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 2, Name = "Leche de Soya sabor Banana Soy", Description = "Lacteos", Brand = "Pil", Price = 8m, Stock = 80, CategoryId = 6, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 3, Name = "Mantequilla con Sal Pil 900 Gr", Description = "Lacteos", Brand = "Regina", Price = 25m, Stock = 40, CategoryId = 6, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 4, Name = "Mantequilla sin Sal Pil 200 Gr", Description = "Lacteos", Brand = "Regina", Price = 18m, Stock = 35, CategoryId = 6, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 5, Name = "Manzana Roja Abasto 500 Gr", Description = "Frutas y Verduras", Brand = "Fresco", Price = 12m, Stock = 100, CategoryId = 3, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 6, Name = "Mandarina Morocochi Abasto 500", Description = "Frutas y Verduras", Brand = "AgroBol", Price = 10m, Stock = 90, CategoryId = 3, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 7, Name = "Rucula Lannin Impex", Description = "Frutas y Verduras", Brand = "Natural", Price = 15m, Stock = 60, CategoryId = 3, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 8, Name = "Naranja Criolla Abasto 500 Gr", Description = "Frutas y Verduras", Brand = "VitaMix", Price = 20m, Stock = 40, CategoryId = 3, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 9, Name = "Manzana Verde Abasto 500 Gr", Description = "Frutas y Verduras", Brand = "FrutiMix", Price = 22m, Stock = 35, CategoryId = 3, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 10, Name = "Pasta Dental Limpieza 180 Gr", Description = "Cuidado personal", Brand = "Colgate", Price = 18m, Stock = 70, CategoryId = 5, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 11, Name = "Hilo Dental con Fluor & Menta Colg", Description = "Cuidado personal", Brand = "Listerine", Price = 25m, Stock = 50, CategoryId = 5, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 12, Name = "Pasta Dental Prot Anti Caries Pep", Description = "Cuidado personal", Brand = "Oral-B", Price = 12m, Stock = 90, CategoryId = 5, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 13, Name = "Cepillo Flex Foramen Unidad", Description = "Cuidado personal", Brand = "Colgate", Price = 10m, Stock = 60, CategoryId = 5, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 14, Name = "Antitranspirante en Barra Speed St", Description = "Cuidado personal", Brand = "Dove", Price = 8m, Stock = 100, CategoryId = 5, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 15, Name = "Desodorante Roll on Antibacterial F", Description = "Cuidado personal", Brand = "Dettol", Price = 15m, Stock = 80, CategoryId = 5, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 16, Name = "Desodorantes y Antitranspirantes", Description = "Cuidado personal", Brand = "Rexona", Price = 20m, Stock = 70, CategoryId = 5, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 17, Name = "Desodorante Roll on Antibacterial F Rexona", Description = "Cuidado personal", Brand = "Rexona", Price = 35m, Stock = 40, CategoryId = 5, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 18, Name = "Prestobarba Gillette 3 Cool 4 Unds", Description = "Cuidado personal", Brand = "Gillette", Price = 25m, Stock = 60, CategoryId = 5, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 19, Name = "Presto Barba Confort 3 Normal Bic", Description = "Cuidado personal", Brand = "Gillette", Price = 22m, Stock = 45, CategoryId = 5, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 20, Name = "Bloqueador Solar Sport Factor 50", Description = "Cuidado personal", Brand = "Nivea", Price = 28m, Stock = 50, CategoryId = 5, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 21, Name = "Crema Depiladora para Piel Sensib", Description = "Cuidado personal", Brand = "Veet", Price = 30m, Stock = 40, CategoryId = 5, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 22, Name = "Jabon Intimo Herbal Nosotras 200", Description = "Cuidado personal", Brand = "Nosotras", Price = 25m, Stock = 60, CategoryId = 5, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 23, Name = "Toalla Normal Maternidad Nosotras", Description = "Cuidado personal", Brand = "Always", Price = 22m, Stock = 50, CategoryId = 5, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 24, Name = "Protectores Diarios", Description = "Cuidado personal", Brand = "Nosotras", Price = 15m, Stock = 70, CategoryId = 5, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 25, Name = "Jabon intimo Nosotras", Description = "Cuidado personal", Brand = "Nosotras", Price = 18m, Stock = 45, CategoryId = 5, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 26, Name = "Toalla Normal Maternidad Plenitud", Description = "Cuidado personal", Brand = "Plenitud", Price = 40m, Stock = 30, CategoryId = 5, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 27, Name = "Desodorante Max Men Inspired Tornado", Description = "Cuidado personal", Brand = "Durex", Price = 25m, Stock = 40, CategoryId = 5, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 28, Name = "Detergente en polvo Matic Omo 2 k", Description = "Limpieza de Ropa", Brand = "OMO", Price = 20m, Stock = 90, CategoryId = 1, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 29, Name = "Suavizante Caricias Brisa De Prima", Description = "Limpieza de Ropa", Brand = "Downy", Price = 25m, Stock = 80, CategoryId = 1, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 30, Name = "Quitamanchas Vanish Prelavado G", Description = "Limpieza de Ropa", Brand = "Vanish", Price = 22m, Stock = 60, CategoryId = 1, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 31, Name = "Jabon Delicada Uno 210 Gr", Description = "Limpieza de Ropa", Brand = "Ariel", Price = 18m, Stock = 100, CategoryId = 1, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 32, Name = "Ambientador Harmony Glade", Description = "Ambientadores e Insecticidas", Brand = "Glade", Price = 15m, Stock = 70, CategoryId = 1, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 33, Name = "Aerosol Mata Todo Tyson 360 cm", Description = "Ambientadores e Insecticidas", Brand = "Raid", Price = 18m, Stock = 60, CategoryId = 1, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 34, Name = "Lavavajillas Naranja Mr Flash 1050", Description = "Limpieza de Cocina", Brand = "Axion", Price = 14m, Stock = 100, CategoryId = 1, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 35, Name = "Crema Brillametal Brasso Negro", Description = "Limpieza de Cocina", Brand = "Scotch-Brite", Price = 20m, Stock = 80, CategoryId = 1, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 36, Name = "SacaGrasa con Gatillo Salpo", Description = "Limpieza de Cocina", Brand = "Scotch-Brite", Price = 10m, Stock = 90, CategoryId = 1, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 37, Name = "Esponja Ola 3 Unds", Description = "Limpieza de Cocina", Brand = "Elite", Price = 18m, Stock = 100, CategoryId = 1, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 38, Name = "Toalla De Cocina Hogar 1 Un", Description = "Limpieza de Baños", Brand = "Scott", Price = 22m, Stock = 120, CategoryId = 1, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 39, Name = "Papel Higienico Plus Doble h", Description = "Limpieza de Baños", Brand = "Lysoform", Price = 20m, Stock = 90, CategoryId = 1, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 40, Name = "Limpia Baño Ultra Rapido co", Description = "Limpieza de Baños", Brand = "Brilux", Price = 15m, Stock = 70, CategoryId = 1, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 41, Name = "Agua Lavandina Aditiva Mari", Description = "Limpieza del Hogar", Brand = "Clorox", Price = 12m, Stock = 100, CategoryId = 1, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 42, Name = "Disinfectante Bebe Lysoform", Description = "Limpieza del Hogar", Brand = "Lysoform", Price = 14m, Stock = 90, CategoryId = 1, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 43, Name = "Limpia Vidrios con Gatillo Oli", Description = "Limpieza del Hogar", Brand = "CIF", Price = 18m, Stock = 80, CategoryId = 1, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 44, Name = "Crema para Calzado Betun N", Description = "Limpieza del Hogar", Brand = "Windex", Price = 15m, Stock = 70, CategoryId = 1, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 45, Name = "Cepillo Multiuso para Ropa C", Description = "Limpieza para Calzados", Brand = "Nugget", Price = 10m, Stock = 50, CategoryId = 1, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 46, Name = "Escoba Multiuso Clorinda Un", Description = "Limpieza para Calzados", Brand = "Brilux", Price = 12m, Stock = 60, CategoryId = 1, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 47, Name = "Guante Naranja T7 1/2 Master", Description = "Utensilios de Limpieza", Brand = "CleanBag", Price = 8m, Stock = 150, CategoryId = 1, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 48, Name = "Recogedor de Basura con M", Description = "Utensilios de Limpieza", Brand = "Brilux", Price = 10m, Stock = 90, CategoryId = 1, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 49, Name = "Palo Trapeador Movica Unid", Description = "Utensilios de Limpieza", Brand = "Brilux", Price = 12m, Stock = 80, CategoryId = 1, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 50, Name = "Bolsa Rayada 60 cm x 80 cm", Description = "Utensilios de Limpieza", Brand = "Brilux", Price = 14m, Stock = 70, CategoryId = 1, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 51, Name = "Arroz Familiar Caisy 1 Kg", Description = "Arroz", Brand = "Fino", Price = 18m, Stock = 100, CategoryId = 2, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 52, Name = "Arroz Superior", Description = "Arroz", Brand = "Fino", Price = 20m, Stock = 90, CategoryId = 2, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 53, Name = "Arroz Integral y Especiales", Description = "Arroz", Brand = "Fino", Price = 25m, Stock = 70, CategoryId = 2, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 54, Name = "1 bolsa de Frijol de 50gr", Description = "Granos", Brand = "San Jorge", Price = 15m, Stock = 80, CategoryId = 2, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 55, Name = "1 bolsa de Frutos Secos", Description = "Granos", Brand = "FrutBol", Price = 30m, Stock = 50, CategoryId = 2, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 56, Name = "1 bolsa de Arveja 100gr", Description = "Granos", Brand = "San Jorge", Price = 14m, Stock = 90, CategoryId = 2, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 57, Name = "1 bolsa de Garbanzo 45 gr", Description = "Granos", Brand = "San Jorge", Price = 18m, Stock = 70, CategoryId = 2, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 58, Name = "Endulzante con Stevia Equal 50", Description = "Azucar y Endulzantes", Brand = "Guabirá", Price = 12m, Stock = 120, CategoryId = 2, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 59, Name = "Edulcorante de Mesa Liquido", Description = "Azucar y Endulzantes", Brand = "Stevia", Price = 20m, Stock = 60, CategoryId = 2, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 60, Name = "Pasta al Huevo Tagliatelle Ar", Description = "Fideos y Pastas", Brand = "Don Vittorio", Price = 18m, Stock = 100, CategoryId = 2, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 61, Name = "Fideo Codo Rayado Don V", Description = "Fideos y Pastas", Brand = "Don Vittorio", Price = 18m, Stock = 100, CategoryId = 2, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 62, Name = "Fideo Cabello de Angel Don", Description = "Fideos y Pastas", Brand = "Oriental", Price = 25m, Stock = 50, CategoryId = 2, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 63, Name = "Fideos Ramen Sabor Costilla", Description = "Fideos y Pastas", Brand = "Don Vittorio", Price = 28m, Stock = 40, CategoryId = 2, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 64, Name = "Pure de Papas Kris 250 Gr", Description = "Comidas Instantaneas", Brand = "Maggi", Price = 15m, Stock = 60, CategoryId = 2, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 65, Name = "Sopa Familiar Pollos con Fic", Description = "Comidas Instantaneas", Brand = "Knorr", Price = 12m, Stock = 70, CategoryId = 2, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 66, Name = "Sopa Crema Zapallo Knorr 7", Description = "Comidas Instantaneas", Brand = "Maggi", Price = 14m, Stock = 60, CategoryId = 2, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 67, Name = "Sopa de Pollo Maruchan 85", Description = "Comidas Instantaneas", Brand = "Nissin", Price = 10m, Stock = 80, CategoryId = 2, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 68, Name = "Cafe Frasco Clasico Iguaçu", Description = "Desayuno", Brand = "Café Copacabana", Price = 25m, Stock = 50, CategoryId = 2, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 69, Name = "Hojuelas de Avena y Chia Ch", Description = "Desayuno", Brand = "Kellogg's", Price = 22m, Stock = 80, CategoryId = 2, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 70, Name = "Chocolate en polvo Chokolisto", Description = "Desayuno", Brand = "Toddy", Price = 18m, Stock = 70, CategoryId = 2, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 71, Name = "Dulce de Leche Sancor 1 Kg", Description = "Desayuno", Brand = "Pil", Price = 20m, Stock = 60, CategoryId = 2, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 72, Name = "Papa Original Pringles 149 G", Description = "Snacks y Golosinas", Brand = "Lays", Price = 10m, Stock = 90, CategoryId = 2, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 73, Name = "Pipoca sabor Mantequilla Ac", Description = "Snacks y Golosinas", Brand = "Chipilo", Price = 12m, Stock = 100, CategoryId = 2, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 74, Name = "Mix de Frutos Secos Varios I", Description = "Snacks y Golosinas", Brand = "FritoLay", Price = 15m, Stock = 80, CategoryId = 2, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 75, Name = "Nachos de Maiz Tradicional I", Description = "Snacks y Golosinas", Brand = "Doritos", Price = 18m, Stock = 70, CategoryId = 2, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 76, Name = "Aceituna Verde Sachet Ceb", Description = "Conservas", Brand = "Carbonell", Price = 25m, Stock = 50, CategoryId = 2, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 77, Name = "Alcaparras Hemmer 170 Gr", Description = "Conservas", Brand = "Bonduelle", Price = 20m, Stock = 60, CategoryId = 2, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 78, Name = "Atun Ensalada California Rea", Description = "Conservas", Brand = "Bonduelle", Price = 18m, Stock = 60, CategoryId = 2, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 79, Name = "Sardina Salsa de Tomate Sa", Description = "Conservas", Brand = "San Lucas", Price = 25m, Stock = 50, CategoryId = 2, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 80, Name = "Mayonesa light Kris 200 Ml", Description = "Salsas y Condimentos", Brand = "Hellmann's", Price = 18m, Stock = 80, CategoryId = 2, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 81, Name = "Ketchup original Doypack Kr", Description = "Salsas y Condimentos", Brand = "Heinz", Price = 15m, Stock = 80, CategoryId = 2, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 82, Name = "Mostaza Original Kris 400 Gr", Description = "Salsas y Condimentos", Brand = "Heinz", Price = 15m, Stock = 80, CategoryId = 2, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 83, Name = "Lajua Churrasquera B&R 22", Description = "Salsas y Condimentos", Brand = "Don Lucho", Price = 12m, Stock = 90, CategoryId = 2, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 84, Name = "Chunchulines en Bandeja x kg", Description = "Carnes", Brand = "Fridosa", Price = 85m, Stock = 30, CategoryId = 4, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 85, Name = "Resa de Carne Sin Condimento e", Description = "Carnes", Brand = "Fridosa", Price = 45m, Stock = 40, CategoryId = 4, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 86, Name = "Langostinos Precocidos Puerto Az", Description = "Carnes", Brand = "Oceana", Price = 60m, Stock = 30, CategoryId = 4, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 87, Name = "Pechuga de Pollo sin piel en Br", Description = "Carnes", Brand = "Sofia", Price = 35m, Stock = 50, CategoryId = 4, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 88, Name = "Tender de Pollo Sofia 500 Gr", Description = "Carnes", Brand = "Sofia", Price = 90m, Stock = 20, CategoryId = 4, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 89, Name = "Alas de Pollo en Bandeja Sofia", Description = "Carnes", Brand = "San Jacinto", Price = 70m, Stock = 25, CategoryId = 4, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 90, Name = "Producto Activo en Categoria Inactiva", Description = "Hogar", Brand = "TestBrand", Price = 10m, Stock = 10, CategoryId = 7, Status = (byte)1, LastUpdate = DateTime.Now },
                new Product { Id = 91, Name = "Producto Inactivo de Prueba", Description = "Limpieza", Brand = "TestBrand", Price = 5m, Stock = 5, CategoryId = 1, Status = (byte)0, LastUpdate = DateTime.Now }
            };

            context.Products.AddRange(products);
            await context.SaveChangesAsync();
        }

        #region SearchProductsAsync Tests

        [Theory]
        [InlineData("yogurt", 1, "Yogurt Bebible Sabor Durazno")] 
        [InlineData("lacteos", 4, "Yogurt Bebible Sabor Durazno", "Leche de Soya sabor Banana Soy", "Mantequilla con Sal Pil 900 Gr", "Mantequilla sin Sal Pil 200 Gr")] 
        [InlineData("pil", 9, 
            "Dulce de Leche Sancor 1 Kg",
            "Leche de Soya sabor Banana Soy", 
            "Mantequilla con Sal Pil 900 Gr", 
            "Mantequilla sin Sal Pil 200 Gr", 
            "Yogurt Bebible Sabor Durazno" 
        
            )]
        [InlineData("frutas", 5, "Manzana Roja Abasto 500 Gr", "Mandarina Morocochi Abasto 500", "Rucula Lannin Impex", "Naranja Criolla Abasto 500 Gr", "Manzana Verde Abasto 500 Gr")] 
        [InlineData("colgate", 2, "Pasta Dental Limpieza 180 Gr", "Cepillo Flex Foramen Unidad")] 
        [InlineData("rexona", 2, "Desodorantes y Antitranspirantes", "Desodorante Roll on Antibacterial F Rexona")] 
        [InlineData("productos de limpieza", 20,
            "Aerosol Mata Todo Tyson 360 cm", "Agua Lavandina Aditiva Mari", "Ambientador Harmony Glade", "Bolsa Rayada 60 cm x 80 cm",
            "Cepillo Multiuso para Ropa C", "Crema Brillametal Brasso Negro", "Crema para Calzado Betun N", "Detergente en polvo Matic Omo 2 k",
            "Disinfectante Bebe Lysoform", "Escoba Multiuso Clorinda Un", "Esponja Ola 3 Unds", "Guante Naranja T7 1/2 Master",
            "Jabon Delicada Uno 210 Gr", "Lavavajillas Naranja Mr Flash 1050", "Limpia Baño Ultra Rapido co", "Limpia Vidrios con Gatillo Oli",
            "Palo Trapeador Movica Unid", "Papel Higienico Plus Doble h", "Quitamanchas Vanish Prelavado G", "Recogedor de Basura con M"
            )]
        [InlineData("ambientadores", 2, "Ambientador Harmony Glade", "Aerosol Mata Todo Tyson 360 cm")]
        [InlineData("alimentos diversos", 20,
            "1 bolsa de Arveja 100gr", "1 bolsa de Frijol de 50gr", "1 bolsa de Frutos Secos", "1 bolsa de Garbanzo 45 gr",
            "Aceituna Verde Sachet Ceb", "Alcaparras Hemmer 170 Gr", "Arroz Familiar Caisy 1 Kg", "Arroz Integral y Especiales",
            "Arroz Superior", "Atun Ensalada California Rea", "Cafe Frasco Clasico Iguaçu", "Chocolate en polvo Chokolisto",
            "Dulce de Leche Sancor 1 Kg", "Edulcorante de Mesa Liquido", "Endulzante con Stevia Equal 50", "Fideo Cabello de Angel Don",
            "Fideo Codo Rayado Don V", "Fideos Ramen Sabor Costilla", "Hojuelas de Avena y Chia Ch", "Ketchup original Doypack Kr"
            )]
        [InlineData("carnes", 6,
            "Alas de Pollo en Bandeja Sofia", "Chunchulines en Bandeja x kg", "Langostinos Precocidos Puerto Az",
            "Pechuga de Pollo sin piel en Br", "Resa de Carne Sin Condimento e", "Tender de Pollo Sofia 500 Gr"
            )]
        [InlineData("inexistente", 0)]
        [InlineData("", 0)]
        [InlineData("Producto Inactivo de Prueba", 0)]
        [InlineData("Producto Activo en Categoria Inactiva", 0)]
        public async Task SearchProductsAsync_ShouldReturnExpectedProducts(
            string searchTerm, int expectedCount, params string[] expectedNames)
        {
            var context = GetInMemoryDbContext();
            await SeedTestData(context);
            var logger = GetMockLogger();
            var validators = GetMockValidators();
            var service = new ProductService(context, logger.Object, validators.createValidator.Object, validators.updateValidator.Object);

            var result = await service.SearchProductsAsync(searchTerm);
            Assert.NotNull(result);
            var products = result.ToList();
            Assert.Equal(expectedCount, products.Count);

            if (expectedCount > 0)
            {
                foreach (var name in expectedNames)
                {
                    Assert.Contains(products, p => p.Name == name);
                }
            }
        }

        // Test: SearchProductsAsync_ShouldThrowException_WhenDatabaseErrorOccurs
        // Propósito: Verifica que el método SearchProductsAsync lanza una excepción (y la registra)
        [Fact]
        public async Task SearchProductsAsync_ShouldThrowException_WhenDatabaseErrorOccurs()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);
            await context.DisposeAsync();

            var logger = GetMockLogger();
            var validators = GetMockValidators();
            var service = new ProductService(context, logger.Object, validators.createValidator.Object, validators.updateValidator.Object);
            await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            {
                await service.SearchProductsAsync("yogurt");
            });
        }

        #endregion

        #region GetProductByIdAsync (ProductSearchDTO) Tests

        // Test: GetProductByIdAsync_ShouldReturnExpectedProductOrNull
        // Propósito: Verifica que el método GetProductByIdAsync retorna el ProductSearchDTO esperado

        [Theory]
        [InlineData(1, "Yogurt Bebible Sabor Durazno", 10.00, 50, "Lacteos")] 
        [InlineData(999, null, 0, 0, null)] 
        [InlineData(91, null, 0, 0, null)] 
        public async Task GetProductByIdAsync_ShouldReturnExpectedProductOrNull(
            short productId, string expectedName, decimal expectedPrice, short expectedStock, string expectedCategoryName)
        {
            var context = GetInMemoryDbContext();
            await SeedTestData(context);
            var logger = GetMockLogger();
            var validators = GetMockValidators();
            var service = new ProductService(context, logger.Object, validators.createValidator.Object, validators.updateValidator.Object);

            var result = await service.GetProductByIdAsync(productId);

            if (expectedName != null)
            {
                Assert.NotNull(result);
                Assert.Equal(productId, result.Id);
                Assert.Equal(expectedName, result.Name);
                Assert.Equal(expectedPrice, result.Price);
                Assert.Equal(expectedStock, result.Stock);
                Assert.Equal(expectedCategoryName, result.CategoryName);
            }
            else
            {
                Assert.Null(result);
            }
        }

        // Test: GetProductByIdAsync_ShouldThrowException_WhenDatabaseErrorOccurs
        // Propósito: Verifica que el método GetProductByIdAsync lanza una excepción (y la registra)

        [Fact]
        public async Task GetProductByIdAsync_ShouldThrowException_WhenDatabaseErrorOccurs()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);
            await context.DisposeAsync();

            var logger = GetMockLogger();
            var validators = GetMockValidators();
            var service = new ProductService(context, logger.Object, validators.createValidator.Object, validators.updateValidator.Object);
            await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            {
                await service.GetProductByIdAsync(1);
            });
        }

        #endregion

        #region HasStockAsync Tests

        // Test: HasStockAsync_ShouldReturnExpectedResult
        // Propósito: Verifica que el método HasStockAsync retorna el resultado esperado (true/false)

        [Theory]
        [InlineData(1, 40, true)] 
        [InlineData(1, 60, false)]
        [InlineData(3, 30, true)]  
        [InlineData(3, 50, false)] 
        [InlineData(999, 5, false)]
        [InlineData(91, 1, false)] 
        public async Task HasStockAsync_ShouldReturnExpectedResult(short productId, short quantity, bool expectedResult)
        {
            var context = GetInMemoryDbContext();
            await SeedTestData(context);
            var logger = GetMockLogger();
            var validators = GetMockValidators();
            var service = new ProductService(context, logger.Object, validators.createValidator.Object, validators.updateValidator.Object);

            var result = await service.HasStockAsync(productId, quantity);
            Assert.Equal(expectedResult, result);
        }

        // Test: HasStockAsync_ShouldThrowException_WhenDatabaseErrorOccurs
        // Propósito: Verifica que el método HasStockAsync lanza una excepción (y la registra)

        [Fact]
        public async Task HasStockAsync_ShouldThrowException_WhenDatabaseErrorOccurs()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);
            await context.DisposeAsync();

            var logger = GetMockLogger();
            var validators = GetMockValidators();
            var service = new ProductService(context, logger.Object, validators.createValidator.Object, validators.updateValidator.Object);
            await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            {
                await service.HasStockAsync(1, 5);
            });
        }

        #endregion

        #region GetAllProductsAsync Tests

        // Test: GetAllProductsAsync_ShouldReturnAllActiveProducts
        // Propósito: Verifica que el método GetAllProductsAsync retorna todos los productos activos

        [Fact]
        public async Task GetAllProductsAsync_ShouldReturnAllActiveProducts()
        {
            var context = GetInMemoryDbContext();
            await SeedTestData(context);
            var logger = GetMockLogger();
            var validators = GetMockValidators();
            var service = new ProductService(context, logger.Object, validators.createValidator.Object, validators.updateValidator.Object);

            var result = await service.GetAllProductsAsync();

            Assert.NotNull(result);
            var products = result.ToList();

            Assert.Equal(89, products.Count);
            Assert.Contains(products, p => p.Id == 1 && p.Name == "Yogurt Bebible Sabor Durazno" && p.CategoryName == "Lacteos");
            Assert.Contains(products, p => p.Id == 28 && p.Name == "Detergente en polvo Matic Omo 2 k" && p.CategoryName == "Productos de Limpieza");
            Assert.Contains(products, p => p.Id == 89 && p.Name == "Alas de Pollo en Bandeja Sofia" && p.CategoryName == "Carnes");
            Assert.DoesNotContain(products, p => p.Id == 90); 
            Assert.DoesNotContain(products, p => p.Id == 91); 
        }

        // Test: GetAllProductsAsync_ShouldReturnEmpty_WhenNoProductsExist
        // Propósito: Verifica que el método GetAllProductsAsync retorna una lista vacía

        [Fact]
        public async Task GetAllProductsAsync_ShouldReturnEmpty_WhenNoProductsExist()
        {
            var context = GetInMemoryDbContext();
            var logger = GetMockLogger();
            var validators = GetMockValidators();
            var service = new ProductService(context, logger.Object, validators.createValidator.Object, validators.updateValidator.Object);

            var result = await service.GetAllProductsAsync();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        // Test: GetAllProductsAsync_ShouldThrowException_WhenDatabaseErrorOccurs
        // Propósito: Verifica que el método GetAllProductsAsync lanza una excepción (y la registra)
        //            cuando ocurre un error inesperado en la base de datos.
        [Fact]
        public async Task GetAllProductsAsync_ShouldThrowException_WhenDatabaseErrorOccurs()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);
            await context.DisposeAsync();

            var logger = GetMockLogger();
            var validators = GetMockValidators();
            var service = new ProductService(context, logger.Object, validators.createValidator.Object, validators.updateValidator.Object);

            await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            {
                await service.GetAllProductsAsync();
            });
        }

        #endregion

        #region GetProductDetailsByIdAsync Tests

        // Test: GetProductDetailsByIdAsync_ShouldReturnExpectedProductOrNull
        // Propósito: Verifica que el método GetProductDetailsByIdAsync retorna el ProductDTO completo esperado
        //            para un producto activo o inactivo, y retorna null para un producto inexistente.
        [Theory]
        [InlineData(1, "Yogurt Bebible Sabor Durazno", (byte)1)]
        [InlineData(999, null, (byte)0)] 
        [InlineData(91, "Producto Inactivo de Prueba", (byte)0)] 
        public async Task GetProductDetailsByIdAsync_ShouldReturnExpectedProductOrNull(
            short productId, string expectedName, byte expectedStatus)
        {
            var context = GetInMemoryDbContext();
            await SeedTestData(context);
            var logger = GetMockLogger();
            var validators = GetMockValidators();
            var service = new ProductService(context, logger.Object, validators.createValidator.Object, validators.updateValidator.Object);

            var result = await service.GetProductDetailsByIdAsync(productId);

            if (expectedName != null)
            {
                Assert.NotNull(result);
                Assert.Equal(productId, result.Id);
                Assert.Equal(expectedName, result.Name);
                Assert.Equal(expectedStatus, result.Status);
            }
            else
            {
                Assert.Null(result);
            }
        }

        // Test: GetProductDetailsByIdAsync_ShouldThrowException_WhenDatabaseErrorOccurs
        // Propósito: Verifica que el método GetProductDetailsByIdAsync lanza una excepción (y la registra)
 
        [Fact]
        public async Task GetProductDetailsByIdAsync_ShouldThrowException_WhenDatabaseErrorOccurs()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);
            await context.DisposeAsync();

            var logger = GetMockLogger();
            var validators = GetMockValidators();
            var service = new ProductService(context, logger.Object, validators.createValidator.Object, validators.updateValidator.Object);

            await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            {
                await service.GetProductDetailsByIdAsync(1);
            });
        }

        #endregion

        #region CreateProductAsync Tests

        // Test: CreateProductAsync_ShouldCreateProduct_WhenAllConditionsAreMet
        // Propósito: Verifica que un producto se crea exitosamente cuando el DTO es válido,
        //            la categoría existe y está activa, y el nombre del producto es único.
        [Fact]
        public async Task CreateProductAsync_ShouldCreateProduct_WhenAllConditionsAreMet()
        {
            var context = GetInMemoryDbContext();
            await SeedTestData(context);
            var logger = GetMockLogger();
            var validators = GetMockValidators();
            var service = new ProductService(context, logger.Object, validators.createValidator.Object, validators.updateValidator.Object);

            var newProductDto = new CreateProductDTO
            {
                Name = "Cereal Avena Integral",
                Description = "Desayuno saludable",
                Brand = "Quaker",
                Price = 4.50m,
                Stock = 20,
                CategoryId = 2 // Alimentos Diversos
            };

            var result = await service.CreateProductAsync(newProductDto);

            Assert.NotNull(result);
            Assert.True(result.Id > 0); // Id generado por la BD
            Assert.Equal("Cereal Avena Integral", result.Name);
            Assert.Equal((byte)1, result.Status); // Se crea como activo
            Assert.Equal(20, result.Stock);

            // Verificar que realmente se añadió a la base de datos
            var productInDb = await context.Products.FindAsync(result.Id);
            Assert.NotNull(productInDb);
            Assert.Equal("Cereal Avena Integral", productInDb.Name);
        }

        // Test: CreateProductAsync - ShouldReturnNull_WhenValidationFails
        // Propósito: Verifica que la creación de un producto falla y retorna null
        //            cuando el DTO de creación no pasa las reglas de validación.
        [Fact]
        public async Task CreateProductAsync_ShouldReturnNull_WhenValidationFails()
        {
            var context = GetInMemoryDbContext();
            await SeedTestData(context);
            var logger = GetMockLogger();
            var validators = GetMockValidators();

            validators.createValidator
                .Setup(v => v.ValidateAsync(It.IsAny<CreateProductDTO>(), default))
                .ReturnsAsync(new ValidationResult(new List<ValidationFailure> { new ValidationFailure("Name", "Name is required") }));

            var invalidProductDto = new CreateProductDTO { Name = "", Description = "Desc", Brand = "Brand", Price = 10, Stock = 5, CategoryId = 1 };
            var service = new ProductService(context, logger.Object, validators.createValidator.Object, validators.updateValidator.Object);
            var result = await service.CreateProductAsync(invalidProductDto);

            Assert.Null(result);
            Assert.Equal(91, await context.Products.CountAsync()); // <-- CORREGIDO AQUÍ: 89 iniciales + 2 de inactividad = 91
        }

        // Test: CreateProductAsync - ShouldReturnNull_WhenCategoryIsInvalid
        // Propósito: Verifica que la creación de un producto falla y retorna null
        //            cuando la CategoryId proporcionada no existe o la categoría está inactiva.
        [Theory]
        [InlineData(999, "Categoría inexistente")] // Categoría inexistente
        [InlineData(7, "Categoría inactiva")] // Categoría inactiva (ID 7)
        public async Task CreateProductAsync_ShouldReturnNull_WhenCategoryIsInvalid(int categoryId, string scenario)
        {
            var context = GetInMemoryDbContext();
            await SeedTestData(context);
            var logger = GetMockLogger();
            var validators = GetMockValidators();
            var service = new ProductService(context, logger.Object, validators.createValidator.Object, validators.updateValidator.Object);

            var newProductDto = new CreateProductDTO
            {
                Name = $"Nuevo Producto {scenario}",
                Description = "Descripción",
                Brand = "Marca",
                Price = 10.00m,
                Stock = 5,
                CategoryId = (byte)categoryId
            };

            var result = await service.CreateProductAsync(newProductDto);

            Assert.Null(result);
            Assert.Equal(91, await context.Products.CountAsync()); // <-- CORREGIDO AQUÍ: 89 iniciales + 2 de inactividad = 91
        }

        // Test: CreateProductAsync - ShouldReturnNull_WhenProductNameAlreadyExists
        // Propósito: Verifica que la creación de un producto falla y retorna null
        //            cuando ya existe otro producto con el mismo nombre.
        [Fact]
        public async Task CreateProductAsync_ShouldReturnNull_WhenProductNameAlreadyExists()
        {
            var context = GetInMemoryDbContext();
            await SeedTestData(context); // Ya existe "Yogurt Bebible Sabor Durazno"
            var logger = GetMockLogger();
            var validators = GetMockValidators();
            var service = new ProductService(context, logger.Object, validators.createValidator.Object, validators.updateValidator.Object);

            var newProductDto = new CreateProductDTO { Name = "Yogurt Bebible Sabor Durazno", Description = "Otro yogurt", Brand = "Nestle", Price = 9, Stock = 10, CategoryId = 6 };
            var result = await service.CreateProductAsync(newProductDto);

            Assert.Null(result);
            Assert.Equal(91, await context.Products.CountAsync()); // <-- CORREGIDO AQUÍ: 89 iniciales + 2 de inactividad = 91
        }

        // Test: CreateProductAsync - ShouldThrowException_WhenDatabaseErrorOccurs
        // Propósito: Verifica que el método CreateProductAsync lanza una excepción (y la registra)
        //            cuando ocurre un error inesperado en la base de datos durante la creación.
        [Fact]
        public async Task CreateProductAsync_ShouldThrowException_WhenDatabaseErrorOccurs()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);
            await context.DisposeAsync();

            var logger = GetMockLogger();
            var validators = GetMockValidators();
            var service = new ProductService(context, logger.Object, validators.createValidator.Object, validators.updateValidator.Object);

            var newProductDto = new CreateProductDTO { Name = "Cereal Avena Integral", Description = "Desayuno saludable", Brand = "Quaker", Price = 4.50m, Stock = 20, CategoryId = 6 };
            await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            {
                await service.CreateProductAsync(newProductDto);
            });
        }

        #endregion

        #region UpdateProductAsync Tests

        // Test: UpdateProductAsync_ShouldUpdateProduct_WhenAllConditionsAreMet
        // Propósito: Verifica que un producto se actualiza exitosamente cuando el DTO es válido,
        //            el producto y la categoría existen y están activos, y el nuevo nombre es único.
        [Fact]
        public async Task UpdateProductAsync_ShouldUpdateProduct_WhenAllConditionsAreMet()
        {
            var context = GetInMemoryDbContext();
            await SeedTestData(context);
            var logger = GetMockLogger();
            var validators = GetMockValidators();
            var service = new ProductService(context, logger.Object, validators.createValidator.Object, validators.updateValidator.Object);

            var updateProductDto = new UpdateProductDTO
            {
                Id = 1, 
                Name = "Yogurt Natural Descremado",
                Description = "Yogurt light sin sabor",
                Brand = "Pil",
                Price = 12.00m,
                Stock = 60,
                CategoryId = 6
            };

            var result = await service.UpdateProductAsync(updateProductDto);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Yogurt Natural Descremado", result.Name);
            Assert.Equal(12.00m, result.Price);
            Assert.Equal(60, result.Stock);

            var productInDb = await context.Products.FindAsync((short)1);
            Assert.NotNull(productInDb);
            Assert.Equal("Yogurt Natural Descremado", productInDb.Name);
            Assert.Equal(12.00m, productInDb.Price);
            Assert.Equal(60, productInDb.Stock);
            Assert.True(productInDb.LastUpdate > DateTime.Now.AddMinutes(-1)); 
        }

        // Test: UpdateProductAsync_ShouldReturnNull_WhenValidationFails
        // Propósito: Verifica que la actualización de un producto falla y retorna null
        //            cuando el DTO de actualización no pasa las reglas de validación.
        [Fact]
        public async Task UpdateProductAsync_ShouldReturnNull_WhenValidationFails()
        {
            var context = GetInMemoryDbContext();
            await SeedTestData(context);
            var logger = GetMockLogger();
            var validators = GetMockValidators();

            validators.updateValidator
                .Setup(v => v.ValidateAsync(It.IsAny<UpdateProductDTO>(), default))
                .ReturnsAsync(new ValidationResult(new List<ValidationFailure> { new ValidationFailure("Name", "Name is required") }));

            var invalidProductDto = new UpdateProductDTO { Id = 1, Name = "", Description = "Desc", Brand = "Brand", Price = 10, Stock = 5, CategoryId = 6 };
            var service = new ProductService(context, logger.Object, validators.createValidator.Object, validators.updateValidator.Object);
            var result = await service.UpdateProductAsync(invalidProductDto);

            Assert.Null(result);
            // Verificar que el producto original no se modificó
            var originalProduct = await context.Products.FindAsync((short)1);
            Assert.Equal("Yogurt Bebible Sabor Durazno", originalProduct.Name);
        }

        // Test: UpdateProductAsync_ShouldReturnNull_WhenUpdateConditionsAreNotMet
        // Propósito: Verifica que la actualización de un producto falla y retorna null
        //            en varios escenarios: producto no existente, categoría inexistente o categoría inactiva.
        [Theory]
        [InlineData(999, "Producto no existente")]
        [InlineData(1, "Categoría inexistente", 999)] 
        [InlineData(1, "Categoría inactiva", 7)]   
        public async Task UpdateProductAsync_ShouldReturnNull_WhenUpdateConditionsAreNotMet(
            short productId, string scenario, int newCategoryId = 6) 
        {
            var context = GetInMemoryDbContext();
            await SeedTestData(context);
            var logger = GetMockLogger();
            var validators = GetMockValidators();
            var service = new ProductService(context, logger.Object, validators.createValidator.Object, validators.updateValidator.Object);

            var updateProductDto = new UpdateProductDTO
            {
                Id = productId,
                Name = "Nombre Actualizado",
                Description = "Desc",
                Brand = "Marca",
                Price = 10.00m,
                Stock = 5,
                CategoryId = (byte)newCategoryId
            };

            var result = await service.UpdateProductAsync(updateProductDto);

            Assert.Null(result);
            if (productId == 1) 
            {
                
                var originalProduct = await context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == (short)productId);
                Assert.NotNull(originalProduct);
                Assert.Equal("Yogurt Bebible Sabor Durazno", originalProduct.Name);
            }
        }

        // Test: UpdateProductAsync_ShouldReturnNull_WhenProductNameAlreadyExistsOnAnotherProduct
        // Propósito: Verifica que la actualización de un producto falla y retorna null
        //            cuando el nuevo nombre ya está en uso por otro producto.
        [Fact]
        public async Task UpdateProductAsync_ShouldReturnNull_WhenProductNameAlreadyExistsOnAnotherProduct()
        {
            var context = GetInMemoryDbContext();
            await SeedTestData(context);
            var logger = GetMockLogger();
            var validators = GetMockValidators();
            var service = new ProductService(context, logger.Object, validators.createValidator.Object, validators.updateValidator.Object);

            var updateProductDto = new UpdateProductDTO
            {
                Id = 1, 
                Name = "Leche de Soya sabor Banana Soy", 
                Description = "Descripción actualizada",
                Brand = "Pil",
                Price = 15.00m,
                Stock = 55,
                CategoryId = 6
            };

            var result = await service.UpdateProductAsync(updateProductDto);

            Assert.Null(result);
            var originalProduct = await context.Products.FindAsync((short)1);
            Assert.Equal("Yogurt Bebible Sabor Durazno", originalProduct.Name);
        }

        // Test: UpdateProductAsync_ShouldSucceed_WhenProductNameIsSameButItsOwnProduct
        // Propósito: Verifica que la actualización de un producto es exitosa incluso si el nombre
        //            no cambia (es decir, el mismo nombre es del propio producto que se está actualizando).
        [Fact]
        public async Task UpdateProductAsync_ShouldSucceed_WhenProductNameIsSameButItsOwnProduct()
        {
            var context = GetInMemoryDbContext();
            await SeedTestData(context); 
            var logger = GetMockLogger();
            var validators = GetMockValidators();
            var service = new ProductService(context, logger.Object, validators.createValidator.Object, validators.updateValidator.Object);

            var updateProductDto = new UpdateProductDTO { Id = 1, Name = "Yogurt Bebible Sabor Durazno", Description = "Descripción actualizada", Brand = "Pil", Price = 10.50m, Stock = 52, CategoryId = 6 };
            var result = await service.UpdateProductAsync(updateProductDto);

            Assert.NotNull(result);
            Assert.Equal("Yogurt Bebible Sabor Durazno", result.Name);
            Assert.Equal("Descripción actualizada", result.Description);
        }

        // Test: UpdateProductAsync_ShouldThrowException_WhenDatabaseErrorOccurs
        // Propósito: Verifica que el método UpdateProductAsync lanza una excepción (y la registra)
        //            cuando ocurre un error inesperado en la base de datos durante la actualización.
        [Fact]
        public async Task UpdateProductAsync_ShouldThrowException_WhenDatabaseErrorOccurs()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);
            await context.DisposeAsync();

            var logger = GetMockLogger();
            var validators = GetMockValidators();
            var service = new ProductService(context, logger.Object, validators.createValidator.Object, validators.updateValidator.Object);

            var updateProductDto = new UpdateProductDTO { Id = 1, Name = "Yogurt Bebible Sabor Durazno", Description = "Desc", Brand = "Marca", Price = 10, Stock = 5, CategoryId = 6 };
            await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            {
                await service.UpdateProductAsync(updateProductDto);
            });
        }

        #endregion

        #region DeleteProductAsync Tests (Borrado Lógico)

        // Test: DeleteProductAsync_ShouldReturnExpectedResult_AndPerformLogicalDelete
        // Propósito: Verifica que el borrado lógico de un producto es exitoso para un producto existente
        //            (cambiando su Status a 0) y que falla (retornando false) para un producto inexistente.
        [Theory]
        [InlineData(1, true)] 
        [InlineData(999, false)] 
        public async Task DeleteProductAsync_ShouldReturnExpectedResult_AndPerformLogicalDelete(short productId, bool expectedResult)
        {
            var context = GetInMemoryDbContext();
            await SeedTestData(context);
            var logger = GetMockLogger();
            var validators = GetMockValidators();
            var service = new ProductService(context, logger.Object, validators.createValidator.Object, validators.updateValidator.Object);

            var result = await service.DeleteProductAsync(productId);

            Assert.Equal(expectedResult, result);

            if (expectedResult)
            {
                var productInDb = await context.Products.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == productId);
                Assert.NotNull(productInDb);
                Assert.Equal((byte)0, productInDb.Status);
            }
        }

        // Test: DeleteProductAsync_ShouldThrowException_WhenDatabaseErrorOccurs
        // Propósito: Verifica que el método DeleteProductAsync lanza una excepción (y la registra)
        //            cuando ocurre un error inesperado en la base de datos durante el borrado.
        [Fact]
        public async Task DeleteProductAsync_ShouldThrowException_WhenDatabaseErrorOccurs()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);
            await context.DisposeAsync();

            var logger = GetMockLogger();
            var validators = GetMockValidators();
            var service = new ProductService(context, logger.Object, validators.createValidator.Object, validators.updateValidator.Object);

            await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            {
                await service.DeleteProductAsync(1);
            });
        }

        #endregion
    }
}