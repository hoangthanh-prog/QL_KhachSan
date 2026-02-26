using System.ComponentModel.DataAnnotations;

namespace QL_KhachSan.Models
{
    [MetadataType(typeof(BienThePhongMetadata))]
    public partial class BienThePhong
    {
        // Không cần code gì thêm
    }

    public class BienThePhongMetadata
    {
        [Required(ErrorMessage = "Vui lòng nhập tên biến thể phòng")]
        [StringLength(100, ErrorMessage = "Tên biến thể phòng không được vượt quá 100 ký tự")]
        [Display(Name = "Biến thể phòng")]
        public string BienThe { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mô tả phòng")]
        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        [Display(Name = "Mô tả")]
        public string MoTa { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá phòng")]
        [Range(0, 100000000, ErrorMessage = "Giá phòng phải lớn hơn hoặc bằng 0")]
        [Display(Name = "Giá bán (VNĐ / đêm)")]
        public decimal? GiaBan { get; set; }
    }
}
