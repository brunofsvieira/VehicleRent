using VehicleRent.Services.Exceptions;

namespace VehicleRent.Infrastructure
{
    public static class FrontendErrorMessages
    {
        public static string ToPt(string? errorCode)
        {
            return errorCode switch
            {
                BusinessErrorCodes.VehicleLicensePlateAlreadyExists => "Já existe um veículo com esta matrícula.",
                BusinessErrorCodes.VehicleBrandRequired => "A marca e obrigatoria.",
                BusinessErrorCodes.VehicleModelRequired => "O modelo e obrigatório.",
                BusinessErrorCodes.VehicleLicensePlateRequired => "A matrícula e obrigatória.",
                BusinessErrorCodes.VehicleLicensePlateInvalidFormat => "Formato de matrícula inválido.",
                BusinessErrorCodes.VehicleFuelInvalid => "Selecione um tipo de combustivel válido.",
                BusinessErrorCodes.VehicleManufacturingYearInvalid => "Ano de fabricacao inválido.",

                BusinessErrorCodes.ClientEmailAlreadyExists => "Já existe um cliente com este email.",
                BusinessErrorCodes.ClientDriverLicenseAlreadyExists => "Já existe um cliente com esta carta de conducão.",
                BusinessErrorCodes.ClientEmailOrDriverLicenseAlreadyExists => "Já existe um cliente com este email ou carta de conducão.",
                BusinessErrorCodes.ClientNameRequired => "O nome completo e obrigatório.",
                BusinessErrorCodes.ClientNameTooLong => "O nome completo não pode ter mais de 50 caracteres.",
                BusinessErrorCodes.ClientEmailRequired => "O email e obrigatório.",
                BusinessErrorCodes.ClientEmailTooLong => "O email não pode ter mais de 100 caracteres.",
                BusinessErrorCodes.ClientEmailInvalidFormat => "Formato de email inválido.",
                BusinessErrorCodes.ClientPhoneRequired => "O telefone e obrigatório.",
                BusinessErrorCodes.ClientPhoneInvalidFormat => "Formato de telefone inválido. Use +<codigo-pais><9digitos>.",
                BusinessErrorCodes.ClientDriverLicenseRequired => "A carta de Condução e obrigatoria.",

                BusinessErrorCodes.RentalVehicleOverlap => "O Veículo Já tem um contrato sobreposto neste periodo.",
                BusinessErrorCodes.RentalSaveFailed => "não foi possivel guardar o contrato de aluguer.",
                BusinessErrorCodes.RentalUpdateFailed => "Não foi possivel atualizar o contrato de aluguer.",
                BusinessErrorCodes.RentalClientNotFound => "O cliente selecionado não existe.",
                BusinessErrorCodes.RentalVehicleNotFound => "O veículo selecionado não existe.",
                BusinessErrorCodes.RentalClientRequired => "Selecione um cliente.",
                BusinessErrorCodes.RentalVehicleRequired => "Selecione um veículo.",
                BusinessErrorCodes.RentalStartDatePast => "A data de inicio não pode ser anterior a hoje.",
                BusinessErrorCodes.RentalEndDateInvalid => "A data de fim tem de ser posterior a data de inicio.",
                BusinessErrorCodes.RentalInitialMileageInvalid => "A quilometragem inicial tem de ser igual ou superior a 0.",

                _ => "não foi possivel concluir a operacão. Verifique os dados e tente novamente."
            };
        }
    }
}
