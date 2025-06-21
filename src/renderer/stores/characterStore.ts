import { create } from 'zustand';
import { devtools, persist } from 'zustand/middleware';
import { Character, CharacterSkills, AuthenticatedCharacter } from '../../shared/types';

interface CharacterState {
  // Authentication state
  characters: AuthenticatedCharacter[];
  activeCharacterId: number | null;
  isAuthenticated: boolean;
  
  // Character data
  characterData: Record<number, Character>;
  skillsData: Record<number, CharacterSkills>;
  
  // Loading states
  isLoading: boolean;
  isSkillsLoading: boolean;
  isSyncing: boolean;
  
  // Error states
  error: string | null;
  syncErrors: Record<number, string>;
  
  // Actions
  addCharacter: (character: AuthenticatedCharacter) => void;
  removeCharacter: (characterId: number) => void;
  setActiveCharacter: (characterId: number) => void;
  updateCharacterData: (characterId: number, data: Character) => void;
  updateSkillsData: (characterId: number, skills: CharacterSkills) => void;
  syncCharacterData: (characterId: number) => Promise<void>;
  syncAllCharacters: () => Promise<void>;
  clearCharacterData: () => void;
  setLoading: (loading: boolean) => void;
  setSkillsLoading: (loading: boolean) => void;
  setSyncing: (syncing: boolean) => void;
  setError: (error: string | null) => void;
  setSyncError: (characterId: number, error: string | null) => void;
}

export const useCharacterStore = create<CharacterState>()(
  devtools(
    persist(
      (set, get) => ({
        // Initial state
        characters: [],
        activeCharacterId: null,
        isAuthenticated: false,
        characterData: {},
        skillsData: {},
        isLoading: false,
        isSkillsLoading: false,
        isSyncing: false,
        error: null,
        syncErrors: {},

        // Actions
        addCharacter: (character) => {
          const characters = [...get().characters];
          const existingIndex = characters.findIndex(c => c.characterId === character.characterId);
          
          if (existingIndex >= 0) {
            characters[existingIndex] = character;
          } else {
            characters.push(character);
          }
          
          set({
            characters,
            isAuthenticated: characters.length > 0,
            activeCharacterId: get().activeCharacterId || character.characterId,
          });
        },

        removeCharacter: (characterId) => {
          const characters = get().characters.filter(c => c.characterId !== characterId);
          const characterData = { ...get().characterData };
          const skillsData = { ...get().skillsData };
          const syncErrors = { ...get().syncErrors };
          
          delete characterData[characterId];
          delete skillsData[characterId];
          delete syncErrors[characterId];
          
          set({
            characters,
            characterData,
            skillsData,
            syncErrors,
            isAuthenticated: characters.length > 0,
            activeCharacterId: get().activeCharacterId === characterId 
              ? (characters.length > 0 ? characters[0].characterId : null)
              : get().activeCharacterId,
          });
        },

        setActiveCharacter: (characterId) => {
          const character = get().characters.find(c => c.characterId === characterId);
          if (character) {
            set({ activeCharacterId: characterId });
          }
        },

        updateCharacterData: (characterId, data) => {
          set({
            characterData: {
              ...get().characterData,
              [characterId]: data,
            },
          });
        },

        updateSkillsData: (characterId, skills) => {
          set({
            skillsData: {
              ...get().skillsData,
              [characterId]: skills,
            },
          });
        },

        syncCharacterData: async (characterId) => {
          const character = get().characters.find(c => c.characterId === characterId);
          if (!character) {
            get().setSyncError(characterId, 'Character not found');
            return;
          }

          set({ isSyncing: true });
          get().setSyncError(characterId, null);

          try {
            // TODO: Implement ESI API calls
            // This is a placeholder for the actual ESI integration
            
            // Simulate API call delay
            await new Promise(resolve => setTimeout(resolve, 1000));
            
            // Mock character data update
            const mockCharacterData: Character = {
              id: characterId,
              name: character.characterName,
              corporationId: character.corporationId,
              allianceId: character.allianceId,
              birthday: '2023-01-01',
              gender: 'male',
              raceId: 1,
              bloodlineId: 1,
              ancestryId: 1,
              securityStatus: 0.0,
              totalSp: 50000000,
              unallocatedSp: 100000,
              isMain: character.isMain,
              lastUpdated: new Date(),
            };

            get().updateCharacterData(characterId, mockCharacterData);
            
          } catch (error) {
            const errorMessage = error instanceof Error ? error.message : 'Failed to sync character data';
            get().setSyncError(characterId, errorMessage);
          } finally {
            set({ isSyncing: false });
          }
        },

        syncAllCharacters: async () => {
          const characters = get().characters;
          if (characters.length === 0) return;

          set({ isSyncing: true });

          try {
            await Promise.all(
              characters.map(character => get().syncCharacterData(character.characterId))
            );
          } catch (error) {
            console.error('Failed to sync all characters:', error);
          } finally {
            set({ isSyncing: false });
          }
        },

        clearCharacterData: () => {
          set({
            characters: [],
            activeCharacterId: null,
            isAuthenticated: false,
            characterData: {},
            skillsData: {},
            syncErrors: {},
            error: null,
          });
        },

        setLoading: (loading) => set({ isLoading: loading }),
        setSkillsLoading: (loading) => set({ isSkillsLoading: loading }),
        setSyncing: (syncing) => set({ isSyncing: syncing }),
        setError: (error) => set({ error }),
        setSyncError: (characterId, error) => {
          const syncErrors = { ...get().syncErrors };
          if (error) {
            syncErrors[characterId] = error;
          } else {
            delete syncErrors[characterId];
          }
          set({ syncErrors });
        },
      }),
      {
        name: 'character-store',
        partialize: (state) => ({
          characters: state.characters,
          activeCharacterId: state.activeCharacterId,
          isAuthenticated: state.isAuthenticated,
          characterData: state.characterData,
          skillsData: state.skillsData,
        }),
      }
    ),
    {
      name: 'character-store',
    }
  )
);