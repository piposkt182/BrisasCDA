using System.Text.Json.Serialization;

namespace Application.Utilities.Dtos
{
    public class VerificationAgreementDto
    {
        [JsonPropertyName("No CDA")]
        public string? NoCda { get; set; }

        [JsonPropertyName("NOMBRE CDA")]
        public string? NombreCda { get; set; }

        [JsonPropertyName("NIT CDA")]
        public string? NitCda { get; set; }

        [JsonPropertyName("DIRECCION CDA")]
        public string? DireccionCda { get; set; }

        [JsonPropertyName("TELEFONO 1  CDA")]
        public string? Telefono1Cda { get; set; }

        [JsonPropertyName("TELEFONO 2  CDA")]
        public string? Telefono2Cda { get; set; }

        [JsonPropertyName("CIUDAD CDA")]
        public string? CiudadCda { get; set; }

        [JsonPropertyName("No RESOLUCION CDA")]
        public string? NoResolucionCda { get; set; }

        [JsonPropertyName("FECHA RESOLUCION CDA")]
        public string? FechaResolucionCda { get; set; }

        [JsonPropertyName("SERIE OPACÍMETRO")]
        public string? SerieOpacimetro { get; set; }

        [JsonPropertyName("MARCA OPACÍMETRO")]
        public string? MarcaOpacimetro { get; set; }

        [JsonPropertyName("NOMBRE DEL PROGRAMA")]
        public string? NombrePrograma { get; set; }

        [JsonPropertyName("VERSION PROGRAMA")]
        public string? VersionPrograma { get; set; }

        [JsonPropertyName("No DE CONSECUTIVO PRUEBA")]
        public string? NoConsecutivoPrueba { get; set; }

        [JsonPropertyName("FECHA Y HORA INICIO DE LA PRUEBA")]
        public string? FechaHoraInicioPrueba { get; set; }

        [JsonPropertyName("FECHA Y HORA FINAL DE LA PRUEBA")]
        public string? FechaHoraFinalPrueba { get; set; }

        [JsonPropertyName("FECHA Y HORA ABORTO DE LA PRUEBA")]
        public string? FechaHoraAbortoPrueba { get; set; }

        [JsonPropertyName("INSPECTOR QUE REALIZA LA PRUEBA")]
        public string? InspectorPrueba { get; set; }

        [JsonPropertyName("HUMEDAD RELATIVA")]
        public string? HumedadRelativa { get; set; }

        [JsonPropertyName("TEMPERATURA AMBIENTE")]
        public string? TemperaturaAmbiente { get; set; }

        [JsonPropertyName("CAUSAL DEL ABORTO DE LA PRUEBA")]
        public string? CausalAborto { get; set; }

        [JsonPropertyName("NOMBRE COMPLETO / RAZÓN SOCIAL PROPIETARIO")]
        public string? NombrePropietario { get; set; }

        [JsonPropertyName("TPO DOCUMENTO")]
        public string? TipoDocumento { get; set; }

        [JsonPropertyName("NO. DOCUMENTO DE IDENTIFICACIÒN")]
        public string? DocumentoIdentificacion { get; set; }

        [JsonPropertyName("DIRECCION")]
        public string? DireccionPropietario { get; set; }

        [JsonPropertyName("TELEFONO")]
        public string? TelefonoPropietario { get; set; }

        [JsonPropertyName("CIUDAD")]
        public string? CiudadPropietario { get; set; }

        [JsonPropertyName("MARCA")]
        public string? MarcaVehiculo { get; set; }

        [JsonPropertyName("LINEA")]
        public string? LineaVehiculo { get; set; }

        [JsonPropertyName("AÑO MODELO")]
        public string? AnioModelo { get; set; }

        [JsonPropertyName("PLACA")]
        public string? Placa { get; set; }

        [JsonPropertyName("CILINDRAJE EN cm³")]
        public string? Cilindraje { get; set; }

        [JsonPropertyName("CLASE DE VEHICULO")]
        public string? ClaseVehiculo { get; set; }

        [JsonPropertyName("SERVICIO")]
        public string? Servicio { get; set; }

        [JsonPropertyName("COMBUSTIBLE")]
        public string? Combustible { get; set; }

        [JsonPropertyName("No MOTOR")]
        public string? NoMotor { get; set; }

        [JsonPropertyName("NUMERO VIN O SERIE")]
        public string? Vin { get; set; }

        [JsonPropertyName("NUMERO DE LICENCIA TRANSITO")]
        public string? LicenciaTransito { get; set; }

        [JsonPropertyName("MODIFICACIONES  AL MOTOR")]
        public string? ModificacionesMotor { get; set; }

        [JsonPropertyName("KILOMETRAJE (Km)")]
        public string? Kilometraje { get; set; }

        [JsonPropertyName("POTENCIA DEL MOTOR")]
        public string? PotenciaMotor { get; set; }

        [JsonPropertyName("RESULTADO FINAL (PROMEDIO)")]
        public string? ResultadoFinal { get; set; }

        [JsonPropertyName("RESULTADO  DE LA PRUEBA")]
        public string? ResultadoPrueba { get; set; }
    }
}
