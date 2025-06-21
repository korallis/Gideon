import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { queryKeys, queryErrorHandlers } from './queryClient';
import { Character, CharacterSkills, AuthenticatedCharacter } from '../../shared/types';
import { useCharacterStore } from '../stores';

// Mock ESI service - will be replaced with actual implementation
const mockESIService = {
  getCharacter: async (characterId: number): Promise<Character> => {
    // Simulate API delay
    await new Promise(resolve => setTimeout(resolve, 500));
    
    return {
      id: characterId,
      name: `Character ${characterId}`,
      corporationId: 1000001,
      allianceId: 99000001,
      birthday: '2023-01-01',
      gender: 'male',
      raceId: 1,
      bloodlineId: 1,
      ancestryId: 1,
      securityStatus: 0.0,
      totalSp: 50000000,
      unallocatedSp: 100000,
      isMain: true,
      lastUpdated: new Date(),
    };
  },
  
  getCharacterSkills: async (characterId: number, accessToken: string): Promise<CharacterSkills> => {
    await new Promise(resolve => setTimeout(resolve, 300));
    
    return {
      characterId,
      skills: [
        {
          skillId: 3300,
          name: 'Gunnery',
          groupId: 255,
          skillPointsInSkill: 1280000,
          trainedSkillLevel: 5,
          activeSkillLevel: 5,
          rank: 1,
          primaryAttribute: 'perception',
          secondaryAttribute: 'willpower',
        },
      ],
      totalSp: 50000000,
      unallocatedSp: 100000,
      attributes: {
        intelligence: 20,
        memory: 20,
        charisma: 19,
        perception: 23,
        willpower: 22,
      },
      implants: [],
      skillQueue: [],
    };
  },
  
  verifyToken: async (accessToken: string): Promise<{ characterId: number; scopes: string[] }> => {
    await new Promise(resolve => setTimeout(resolve, 200));
    
    return {
      characterId: 123456789,
      scopes: ['esi-skills.read_skills.v1', 'esi-characters.read_character_info.v1'],
    };
  },
};

// Character data queries
export const useCharacterQuery = (characterId: number, enabled = true) => {
  return useQuery({
    queryKey: queryKeys.characters.detail(characterId),
    queryFn: () => mockESIService.getCharacter(characterId),
    enabled: enabled && characterId > 0,
    staleTime: 10 * 60 * 1000, // Character data doesn't change often
    onError: (error) => {
      queryErrorHandlers.handleGenericError(error, queryKeys.characters.detail(characterId));
    },
  });
};

// Character skills query
export const useCharacterSkillsQuery = (characterId: number, accessToken?: string) => {
  return useQuery({
    queryKey: queryKeys.characters.skills(characterId),
    queryFn: () => {
      if (!accessToken) {
        throw new Error('Access token required for skills data');
      }
      return mockESIService.getCharacterSkills(characterId, accessToken);
    },
    enabled: Boolean(characterId && accessToken),
    staleTime: 5 * 60 * 1000, // Skills update more frequently
    onError: (error) => {
      queryErrorHandlers.handleAuthError(error);
      queryErrorHandlers.handleGenericError(error, queryKeys.characters.skills(characterId));
    },
  });
};

// Token verification query
export const useTokenVerificationQuery = (accessToken: string) => {
  return useQuery({
    queryKey: queryKeys.esi.verify(accessToken),
    queryFn: () => mockESIService.verifyToken(accessToken),
    enabled: Boolean(accessToken),
    staleTime: 15 * 60 * 1000, // Tokens are valid for a while
    retry: false, // Don't retry invalid tokens
    onError: (error) => {
      queryErrorHandlers.handleAuthError(error);
    },
  });
};

// Character attributes query
export const useCharacterAttributesQuery = (characterId: number, accessToken?: string) => {
  return useQuery({
    queryKey: queryKeys.characters.attributes(characterId),
    queryFn: async () => {
      if (!accessToken) {
        throw new Error('Access token required for attributes data');
      }
      
      // Mock attributes data
      await new Promise(resolve => setTimeout(resolve, 200));
      return {
        intelligence: 20,
        memory: 20,
        charisma: 19,
        perception: 23,
        willpower: 22,
        bonusRemaps: 2,
        lastRemapDate: new Date('2023-01-01'),
        accrueRemap: new Date('2024-01-01'),
      };
    },
    enabled: Boolean(characterId && accessToken),
    staleTime: 24 * 60 * 60 * 1000, // Attributes change rarely
  });
};

// Character implants query
export const useCharacterImplantsQuery = (characterId: number, accessToken?: string) => {
  return useQuery({
    queryKey: queryKeys.characters.implants(characterId),
    queryFn: async () => {
      if (!accessToken) {
        throw new Error('Access token required for implants data');
      }
      
      // Mock implants data
      await new Promise(resolve => setTimeout(resolve, 200));
      return [
        {
          implantId: 9899,
          name: 'Ocular Filter - Basic',
          slot: 1,
          attributes: { perception: 1 },
        },
      ];
    },
    enabled: Boolean(characterId && accessToken),
    staleTime: 30 * 60 * 1000, // Implants don't change often
  });
};

// Character skill queue query
export const useCharacterSkillQueueQuery = (characterId: number, accessToken?: string) => {
  return useQuery({
    queryKey: queryKeys.characters.skillQueue(characterId),
    queryFn: async () => {
      if (!accessToken) {
        throw new Error('Access token required for skill queue data');
      }
      
      // Mock skill queue data
      await new Promise(resolve => setTimeout(resolve, 200));
      return [
        {
          skillId: 3300,
          queuedLevel: 5,
          trainingStartSp: 1280000,
          levelStartSp: 1280000,
          levelEndSp: 7200000,
          startDate: new Date(),
          finishDate: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000), // 7 days
        },
      ];
    },
    enabled: Boolean(characterId && accessToken),
    staleTime: 60 * 1000, // Skill queue is very dynamic
    refetchInterval: 5 * 60 * 1000, // Auto-refetch every 5 minutes
  });
};

// Character mutations
export const useUpdateCharacterMutation = () => {
  const queryClient = useQueryClient();
  const { updateCharacterData } = useCharacterStore();
  
  return useMutation({
    mutationFn: async ({ characterId, updates }: { characterId: number; updates: Partial<Character> }) => {
      // Mock API call
      await new Promise(resolve => setTimeout(resolve, 500));
      return { characterId, updates };
    },
    onMutate: async ({ characterId, updates }) => {
      // Cancel any outgoing refetches
      await queryClient.cancelQueries({ queryKey: queryKeys.characters.detail(characterId) });
      
      // Snapshot the previous value
      const previousCharacter = queryClient.getQueryData(queryKeys.characters.detail(characterId));
      
      // Optimistically update
      queryClient.setQueryData(
        queryKeys.characters.detail(characterId),
        (old: Character | undefined) => old ? { ...old, ...updates } : undefined
      );
      
      // Update store
      if (previousCharacter) {
        updateCharacterData(characterId, { ...previousCharacter as Character, ...updates });
      }
      
      return { previousCharacter };
    },
    onError: (err, { characterId }, context) => {
      // Rollback on error
      if (context?.previousCharacter) {
        queryClient.setQueryData(queryKeys.characters.detail(characterId), context.previousCharacter);
        updateCharacterData(characterId, context.previousCharacter as Character);
      }
    },
    onSettled: (data, error, { characterId }) => {
      // Always refetch after error or success
      queryClient.invalidateQueries({ queryKey: queryKeys.characters.detail(characterId) });
    },
  });
};

// Refresh character data mutation
export const useRefreshCharacterMutation = () => {
  const queryClient = useQueryClient();
  const { setSyncing, setSyncError } = useCharacterStore();
  
  return useMutation({
    mutationFn: async (characterId: number) => {
      setSyncing(true);
      setSyncError(characterId, null);
      
      try {
        // Fetch fresh data from ESI
        const [character, skills] = await Promise.all([
          mockESIService.getCharacter(characterId),
          // mockESIService.getCharacterSkills(characterId, accessToken), // TODO: Get token
        ]);
        
        return { character, skills: null };
      } catch (error) {
        setSyncError(characterId, error instanceof Error ? error.message : 'Sync failed');
        throw error;
      } finally {
        setSyncing(false);
      }
    },
    onSuccess: ({ character }, characterId) => {
      // Update cache with fresh data
      queryClient.setQueryData(queryKeys.characters.detail(characterId), character);
      
      // Update store
      const { updateCharacterData } = useCharacterStore.getState();
      updateCharacterData(characterId, character);
    },
    onError: (error, characterId) => {
      const errorMessage = error instanceof Error ? error.message : 'Failed to refresh character';
      setSyncError(characterId, errorMessage);
      queryErrorHandlers.handleGenericError(error, queryKeys.characters.detail(characterId));
    },
  });
};

// Bulk character sync mutation
export const useBulkCharacterSyncMutation = () => {
  const queryClient = useQueryClient();
  const { characters, setSyncing } = useCharacterStore();
  
  return useMutation({
    mutationFn: async () => {
      setSyncing(true);
      
      try {
        const syncPromises = characters.map(async (char) => {
          const character = await mockESIService.getCharacter(char.characterId);
          return { characterId: char.characterId, character };
        });
        
        const results = await Promise.allSettled(syncPromises);
        return results;
      } finally {
        setSyncing(false);
      }
    },
    onSuccess: (results) => {
      results.forEach((result) => {
        if (result.status === 'fulfilled') {
          const { characterId, character } = result.value;
          queryClient.setQueryData(queryKeys.characters.detail(characterId), character);
          
          const { updateCharacterData } = useCharacterStore.getState();
          updateCharacterData(characterId, character);
        }
      });
    },
  });
};

// Custom hooks for common patterns
export const useActiveCharacterData = () => {
  const { activeCharacterId, characters } = useCharacterStore();
  const activeCharacter = characters.find(c => c.characterId === activeCharacterId);
  
  const characterQuery = useCharacterQuery(
    activeCharacterId || 0,
    Boolean(activeCharacterId)
  );
  
  const skillsQuery = useCharacterSkillsQuery(
    activeCharacterId || 0,
    activeCharacter?.accessToken
  );
  
  return {
    character: characterQuery.data,
    skills: skillsQuery.data,
    isLoading: characterQuery.isLoading || skillsQuery.isLoading,
    error: characterQuery.error || skillsQuery.error,
    refetch: () => {
      characterQuery.refetch();
      skillsQuery.refetch();
    },
  };
};

// Hook for character with all related data
export const useCharacterWithData = (characterId: number) => {
  const { characters } = useCharacterStore();
  const character = characters.find(c => c.characterId === characterId);
  
  const characterQuery = useCharacterQuery(characterId);
  const skillsQuery = useCharacterSkillsQuery(characterId, character?.accessToken);
  const attributesQuery = useCharacterAttributesQuery(characterId, character?.accessToken);
  const implantsQuery = useCharacterImplantsQuery(characterId, character?.accessToken);
  const skillQueueQuery = useCharacterSkillQueueQuery(characterId, character?.accessToken);
  
  const isLoading = [
    characterQuery,
    skillsQuery,
    attributesQuery,
    implantsQuery,
    skillQueueQuery,
  ].some(query => query.isLoading);
  
  const hasError = [
    characterQuery,
    skillsQuery,
    attributesQuery,
    implantsQuery,
    skillQueueQuery,
  ].some(query => query.error);
  
  return {
    character: characterQuery.data,
    skills: skillsQuery.data,
    attributes: attributesQuery.data,
    implants: implantsQuery.data,
    skillQueue: skillQueueQuery.data,
    isLoading,
    hasError,
    refetchAll: () => {
      characterQuery.refetch();
      skillsQuery.refetch();
      attributesQuery.refetch();
      implantsQuery.refetch();
      skillQueueQuery.refetch();
    },
  };
};