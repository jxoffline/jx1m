using System.Collections.Concurrent;

namespace GameServer.Logic
{
    /// <summary>
    /// Quản lý pet
    /// </summary>
    public static partial class KTPetManager
    {
        /// <summary>
        /// Danh sách Pet
        /// </summary>
        private static readonly ConcurrentDictionary<int, Pet> pets = new ConcurrentDictionary<int, Pet>();

        /// <summary>
        /// Tìm pet theo ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Pet FindPet(int id)
        {
            if (KTPetManager.pets.TryGetValue(id, out Pet pet))
            {
                return pet;
            }
            return null;
        }
    }
}
