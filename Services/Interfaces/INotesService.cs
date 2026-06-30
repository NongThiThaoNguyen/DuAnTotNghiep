using System.Collections.Generic;
using System.Threading.Tasks;
using DuAnTotNghiep.Models.ViewModels.Student;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface INotesService
    {
        Task<List<NoteViewModel>> GetNotesAsync(int userId);
        Task<NoteViewModel?> GetNoteByIdAsync(int id, int userId);
        Task<bool> CreateNoteAsync(int userId, NoteCreateUpdateViewModel model);
        Task<bool> UpdateNoteAsync(int userId, NoteCreateUpdateViewModel model);
        Task<bool> DeleteNoteAsync(int id, int userId);
    }
}
