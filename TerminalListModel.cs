using System.ComponentModel.DataAnnotations;

namespace FPSVideoCallApplication.Models.Terminal
{
    /// <summary>
    /// Модель для отображения списка видеотерминалов
    /// </summary>
    public class TerminalListModel
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Display(Name = "Идентификатор")]
        public int Id { get; set; }

        /// <summary>
        ///  Указывает, назначен ли видеотерминал для общего доступа, иначе - для осужденных
        /// </summary>
        [Display(Name = "Назначение видеотерминала")]
        public bool IsPublic { get; set; }

        /// <summary>
        /// Идентификатор  учреждения
        /// </summary>
        public int CorrectionFacilityId { get; set; }

        /// <summary>
        /// Телефонный номер, присвоенный видеотерминалу
        /// </summary>
        [Display(Name = "Номер видеотерминала")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Адрес установки видеотерминала
        /// </summary>
        [Display(Name = "Адрес установки видеотерминала")]
        public string InstallationAddress { get; set; }

        /// <summary>
        /// Название учреждения, в котором расположен видеотерминал
        /// </summary>
        [Display(Name = "Подразделение")]
        public string  CorrectionFacilityName { get; set; }

        /// <summary>
        /// Идентификатор  региона месторасположения
        /// </summary>
        public int RegionId { get; set; }

        /// <summary>
        ///  Указывает, в рабочем ли состоянии терминал
        /// </summary>
        [Display(Name = "Доступен")]
        public bool IsActive { get; set; }
    }
}