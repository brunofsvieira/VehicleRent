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
                BusinessErrorCodes.VehicleBrandRequired => "A marca é obrigatória.",
                BusinessErrorCodes.VehicleModelRequired => "O modelo é obrigatório.",
                BusinessErrorCodes.VehicleLicensePlateRequired => "A matrícula é obrigatória.",
                BusinessErrorCodes.VehicleLicensePlateInvalidFormat => "Formato de matrícula inválido.",
                BusinessErrorCodes.VehicleFuelInvalid => "Selecione um tipo de combustível válido.",
                BusinessErrorCodes.VehicleManufacturingYearInvalid => "Ano de fabricação inválido.",
                BusinessErrorCodes.VehicleDeleteBlockedActiveRental => "Não é possível apagar um veículo alugado.",

                BusinessErrorCodes.ClientEmailAlreadyExists => "Já existe um cliente com este email.",
                BusinessErrorCodes.ClientDriverLicenseAlreadyExists => "Já existe um cliente com esta carta de condução.",
                BusinessErrorCodes.ClientEmailOrDriverLicenseAlreadyExists => "Já existe um cliente com este email ou carta de condução.",
                BusinessErrorCodes.ClientNameRequired => "O nome completo é obrigatório.",
                BusinessErrorCodes.ClientNameTooLong => "O nome completo não pode ter mais de 50 caracteres.",
                BusinessErrorCodes.ClientEmailRequired => "O email é obrigatório.",
                BusinessErrorCodes.ClientEmailTooLong => "O email não pode ter mais de 100 caracteres.",
                BusinessErrorCodes.ClientEmailInvalidFormat => "Formato de email inválido.",
                BusinessErrorCodes.ClientPhoneRequired => "O telefone é obrigatório.",
                BusinessErrorCodes.ClientPhoneInvalidFormat => "Formato de telefone inválido. Use +<código-pais><9digitos>.",
                BusinessErrorCodes.ClientDriverLicenseRequired => "A carta de condução é obrigatória.",
                BusinessErrorCodes.ClientDeleteBlockedActiveRental => "Não é possível apagar um cliente com aluguer em curso.",

                BusinessErrorCodes.RentalVehicleOverlap => "O veículo ja tem um contrato sobreposto neste periodo.",
                BusinessErrorCodes.RentalSaveFailed => "Não foi possível guardar o contrato de aluguer.",
                BusinessErrorCodes.RentalUpdateFailed => "Não foi possível atualizar o contrato de aluguer.",
                BusinessErrorCodes.RentalClientNotFound => "O cliente selecionado não existe.",
                BusinessErrorCodes.RentalVehicleNotFound => "O veículo selecionado não existe.",
                BusinessErrorCodes.RentalClientRequired => "Selecione um cliente.",
                BusinessErrorCodes.RentalVehicleRequired => "Selecione um veículo.",
                BusinessErrorCodes.RentalStartDatePast => "A data de início não pode ser anterior a hoje.",
                BusinessErrorCodes.RentalEndDateInvalid => "A data de fim tem de ser posterior à data de início.",
                BusinessErrorCodes.RentalInitialMileageInvalid => "A quilometragem inicial tem de ser igual ou superior a 0.",
                BusinessErrorCodes.RentalDeleteBlockedActiveContract => "Não é possível apagar um contrato em curso.",

                _ => "Não foi possivel concluir a operação. Verifique os dados e tente novamente."
            };
        }
    }
}
